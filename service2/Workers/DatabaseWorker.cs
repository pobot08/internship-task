using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using service2.Data;
using service2.DTOs;
using service2.Models;

namespace service2.Workers;

public class DatabaseWorker : BackgroundService
{
    private readonly ILogger<DatabaseWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private IModel? _channel;
    private IConnection? _connection;

    public DatabaseWorker(
        ILogger<DatabaseWorker> logger,
        IServiceScopeFactory scopeFactory,
        IConfiguration config)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _config = config;  // Fix #3: берём хост из конфига
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Fix #3: читаем хост из конфига
        var host = _config["RabbitMQ:Host"] ?? "localhost";
        var factory = new ConnectionFactory { HostName = host };

        for (int i = 0; i < 10; i++)
        {
            try
            {
                _connection = factory.CreateConnection();
                break;
            }
            catch
            {
                _logger.LogWarning("RabbitMQ недоступен, попытка {Attempt}/10...", i + 1);
                Thread.Sleep(3000);
            }
        }

        // Fix #8: явная проверка что подключение установлено
        if (_connection == null)
            throw new Exception("Не удалось подключиться к RabbitMQ после 10 попыток");

        _channel = _connection.CreateModel();
        _channel.QueueDeclare(RabbitMqPublisher.DbQueueName,
            durable: true, exclusive: false, autoDelete: false);
        _channel.BasicQos(0, 1, false);

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());

            try
            {
                var data = JsonSerializer.Deserialize<Service3Response>(message,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });

                if (data != null)
                    await SaveToDatabaseAsync(data);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка сохранения в БД");
                _channel.BasicNack(ea.DeliveryTag, false, requeue: true);
            }
        };

        _channel.BasicConsume(RabbitMqPublisher.DbQueueName, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    private async Task SaveToDatabaseAsync(Service3Response data)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Fix #10: используем асинхронный AnyAsync вместо синхронного Any
        var exists = await db.Batches
            .AnyAsync(b => b.SourceBatchId == data.Envelope.SourceBatchId);

        if (exists)
        {
            _logger.LogWarning("Батч {BatchId} уже есть в БД, пропускаю",
                data.Envelope.SourceBatchId);
            return;
        }

        var batch = new ReceivedBatch
        {
            SourceBatchId = data.Envelope.SourceBatchId,
            TransformedAt = data.Envelope.TransformedAt,
            ItemsCount = data.Envelope.ItemsCount,
            TokensUsed = data.Envelope.TokensUsed,
            ReceivedAt = DateTime.UtcNow,
            Items = data.Items.Select(i => new ReceivedItem
            {
                Uid = i.Uid,
                PayloadHash = i.PayloadHash,
                Payload = i.Payload,
                NumericValue = i.NumericValue,
                PreciseValue = i.PreciseValue,
                TimestampIso = i.TimestampIso
            }).ToList()
        };

        db.Batches.Add(batch);
        await db.SaveChangesAsync();

        _logger.LogInformation("Батч {BatchId} сохранён в БД", data.Envelope.SourceBatchId);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("DatabaseWorker остановлен");
        _channel?.Close();
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}