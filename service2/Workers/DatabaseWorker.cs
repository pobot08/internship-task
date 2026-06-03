using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using service2.Data;
using service2.DTOs;
using service2.Models;

namespace service2.Workers;

public class DatabaseWorker : BackgroundService
{
    private readonly ILogger<DatabaseWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private RabbitMQ.Client.IModel? _channel;
    private IConnection? _connection;

    public DatabaseWorker(
        ILogger<DatabaseWorker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(RabbitMqPublisher.DbQueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.BasicQos(0, 1, false);

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());

            try
            {
                var data = JsonSerializer.Deserialize<Service3Response>(message, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                if (data != null)
                {
                    await SaveToDatabaseAsync(data);
                }

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

        // Идемпотентность — если батч уже есть в БД, пропускаем
        var exists = db.Batches.Any(b => b.SourceBatchId == data.Envelope.SourceBatchId);
        if (exists)
        {
            _logger.LogWarning("Батч {BatchId} уже есть в БД, пропускаю", data.Envelope.SourceBatchId);
            return;
        }

        // Создаём батч
        var batch = new ReceivedBatch
        {
            SourceBatchId = data.Envelope.SourceBatchId,
            TransformedAt = data.Envelope.TransformedAt,
            ItemsCount = data.Envelope.ItemsCount,
            TokensUsed = data.Envelope.TokensUsed,
            // Сохраняем все items
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

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("DatabaseWorker остановлен");
        _channel?.Close();
        await base.StopAsync(cancellationToken);
    }
}