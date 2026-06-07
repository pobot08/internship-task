using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using service2.Data;
using service2.DTOs;
using Microsoft.EntityFrameworkCore;

namespace service2.Workers;

public class ExcelWorker : BackgroundService
{
    private readonly ILogger<ExcelWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;  // Fix #2: нужен для записи пути в БД
    private readonly IConfiguration _config;
    private readonly string _outputDirectory;
    private IModel? _channel;
    private IConnection? _connection;

    public ExcelWorker(
        ILogger<ExcelWorker> logger,
        IServiceScopeFactory scopeFactory,
        IConfiguration config)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _config = config;
        _outputDirectory = config["Consumer:ExcelOutputDirectory"] ?? "/data/excel";
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Fix #3: читаем хост из конфига, не хардкодим
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

        // Fix #8: проверяем что подключение установлено
        if (_connection == null)
            throw new Exception("Не удалось подключиться к RabbitMQ после 10 попыток");

        _channel = _connection.CreateModel();
        _channel.QueueDeclare(RabbitMqPublisher.ExcelQueueName,
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
                {
                    var filePath = GenerateExcel(data);
                    await SaveFilePathToDbAsync(data.Envelope.SourceBatchId, filePath);
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка генерации Excel");
                _channel.BasicNack(ea.DeliveryTag, false, requeue: true);
            }
        };

        _channel.BasicConsume(RabbitMqPublisher.ExcelQueueName, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    private string GenerateExcel(Service3Response data)
    {
        Directory.CreateDirectory(_outputDirectory);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var fileName = $"batch_{data.Envelope.SourceBatchId}_{timestamp}.xlsx";
        var filePath = Path.Combine(_outputDirectory, fileName);

        using var workbook = new XLWorkbook();

        // ===== Лист 1 — Envelope =====
        var sheet1 = workbook.Worksheets.Add("Envelope");

        sheet1.Cell(1, 1).Value = "source_batch_id";
        sheet1.Cell(1, 2).Value = "transformed_at";
        sheet1.Cell(1, 3).Value = "items_count";
        sheet1.Cell(1, 4).Value = "tokens_used";
        sheet1.Row(1).Style.Font.Bold = true;
        sheet1.SheetView.FreezeRows(1);

        sheet1.Cell(2, 1).Value = data.Envelope.SourceBatchId;
        sheet1.Cell(2, 2).Value = data.Envelope.TransformedAt.ToString("O");
        sheet1.Cell(2, 3).Value = data.Envelope.ItemsCount;
        sheet1.Cell(2, 4).Value = data.Envelope.TokensUsed;

        // ===== Лист 2 — Items =====
        var sheet2 = workbook.Worksheets.Add("Items");

        sheet2.Cell(1, 1).Value = "uid";
        sheet2.Cell(1, 2).Value = "payload_hash";
        sheet2.Cell(1, 3).Value = "payload";
        sheet2.Cell(1, 4).Value = "numeric_value";
        sheet2.Cell(1, 5).Value = "precise_value";
        sheet2.Cell(1, 6).Value = "timestamp_iso";
        sheet2.Row(1).Style.Font.Bold = true;
        sheet2.SheetView.FreezeRows(1);

        for (int i = 0; i < data.Items.Count; i++)
        {
            var item = data.Items[i];
            int row = i + 2;

            sheet2.Cell(row, 1).Value = item.Uid;
            sheet2.Cell(row, 2).Value = item.PayloadHash;
            sheet2.Cell(row, 3).Value = item.Payload;
            sheet2.Cell(row, 4).Value = item.NumericValue;
            sheet2.Cell(row, 5).Value = item.PreciseValue;
            sheet2.Cell(row, 6).Value = item.TimestampIso;
        }

        workbook.SaveAs(filePath);
        _logger.LogInformation("Excel создан: {FilePath}", filePath);

        return filePath;
    }

    // Fix #2: обновляем ExcelFilePath в БД после создания файла
    private async Task SaveFilePathToDbAsync(string sourceBatchId, string filePath)
    {
        // DatabaseWorker мог ещё не записать батч — делаем несколько попыток
        for (int attempt = 0; attempt < 5; attempt++)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var batch = await db.Batches
                .FirstOrDefaultAsync(b => b.SourceBatchId == sourceBatchId);

            if (batch != null)
            {
                batch.ExcelFilePath = filePath;
                await db.SaveChangesAsync();
                _logger.LogInformation("ExcelFilePath сохранён для батча {BatchId}", sourceBatchId);
                return;
            }

            // DatabaseWorker ещё не сохранил запись — ждём секунду
            _logger.LogDebug(
                "Батч {BatchId} ещё не в БД, попытка {Attempt}/5",
                sourceBatchId, attempt + 1);
            await Task.Delay(1000);
        }

        _logger.LogWarning(
            "Не удалось сохранить ExcelFilePath для батча {BatchId}: запись не найдена",
            sourceBatchId);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ExcelWorker остановлен");
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