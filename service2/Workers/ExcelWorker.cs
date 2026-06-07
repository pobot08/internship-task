using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using service2.DTOs;

namespace service2.Workers;

public class ExcelWorker : BackgroundService
{
    private readonly ILogger<ExcelWorker> _logger;
    private readonly string _outputDirectory;
    private RabbitMQ.Client.IModel? _channel;
    private IConnection? _connection;

    public ExcelWorker(ILogger<ExcelWorker> logger, IConfiguration config)
    {
        _logger = logger;
        // Читаем папку из конфига
        _outputDirectory = config["Consumer:ExcelOutputDirectory"] ?? "C:/yaro/excel-output";
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { HostName = "rabbitmq" };

        for (int i = 0; i < 10; i++)
        {
            try
            {
                _connection = factory.CreateConnection();
                break;
            }
            catch
            {
                Console.WriteLine($"RabbitMQ недоступен, попытка {i + 1}/10...");
                Thread.Sleep(3000);
            }
        }

        _channel = _connection.CreateModel(); 

        _channel.QueueDeclare(RabbitMqPublisher.ExcelQueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.BasicQos(0, 1, false);

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());

            try
            {
                // Десериализуем JSON в объект
                var data = JsonSerializer.Deserialize<Service3Response>(message, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                if (data != null)
                {
                    GenerateExcel(data);
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

    private void GenerateExcel(Service3Response data)
    {
        // Создаём папку если не существует
        Directory.CreateDirectory(_outputDirectory);

        // Имя файла по заданию: batch_{id}_{дата}.xlsx
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var fileName = $"batch_{data.Envelope.SourceBatchId}_{timestamp}.xlsx";
        var filePath = Path.Combine(_outputDirectory, fileName);

        using var workbook = new XLWorkbook();

        // ===== Лист 1 — Envelope =====
        var sheet1 = workbook.Worksheets.Add("Envelope");

        // Заголовки жирным
        sheet1.Cell(1, 1).Value = "source_batch_id";
        sheet1.Cell(1, 2).Value = "transformed_at";
        sheet1.Cell(1, 3).Value = "items_count";
        sheet1.Cell(1, 4).Value = "tokens_used";
        sheet1.Row(1).Style.Font.Bold = true;

        // Заморозить первую строку
        sheet1.SheetView.FreezeRows(1);

        // Данные
        sheet1.Cell(2, 1).Value = data.Envelope.SourceBatchId;
        sheet1.Cell(2, 2).Value = data.Envelope.TransformedAt.ToString("O");
        sheet1.Cell(2, 3).Value = data.Envelope.ItemsCount;
        sheet1.Cell(2, 4).Value = data.Envelope.TokensUsed;

        // ===== Лист 2 — Items =====
        var sheet2 = workbook.Worksheets.Add("Items");

        // Заголовки жирным
        sheet2.Cell(1, 1).Value = "uid";
        sheet2.Cell(1, 2).Value = "payload_hash";
        sheet2.Cell(1, 3).Value = "payload";
        sheet2.Cell(1, 4).Value = "numeric_value";
        sheet2.Cell(1, 5).Value = "precise_value";
        sheet2.Cell(1, 6).Value = "timestamp_iso";
        sheet2.Row(1).Style.Font.Bold = true;

        // Заморозить первую строку
        sheet2.SheetView.FreezeRows(1);

        // Данные — каждый item в отдельной строке
        for (int i = 0; i < data.Items.Count; i++)
        {
            var item = data.Items[i];
            int row = i + 2; // начинаем со второй строки

            sheet2.Cell(row, 1).Value = item.Uid;
            sheet2.Cell(row, 2).Value = item.PayloadHash;
            sheet2.Cell(row, 3).Value = item.Payload;
            sheet2.Cell(row, 4).Value = item.NumericValue;
            sheet2.Cell(row, 5).Value = item.PreciseValue;
            sheet2.Cell(row, 6).Value = item.TimestampIso;
        }

        workbook.SaveAs(filePath);
        _logger.LogInformation("Excel создан: {FilePath}", filePath);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ExcelWorker остановлен");
        _channel?.Close();
        await base.StopAsync(cancellationToken);
    }
}