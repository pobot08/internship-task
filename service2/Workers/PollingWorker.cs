using service2.Services;

namespace service2.Workers;

public class PollingWorker : BackgroundService
{
    private readonly ILogger<PollingWorker> _logger;
    private readonly Service3Client _client;
    private readonly RabbitMqPublisher _publisher;
    private readonly IConfiguration _config;
    private readonly Random _random = new();

    private static readonly System.Text.Json.JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower
    };

    public PollingWorker(
        ILogger<PollingWorker> logger,
        Service3Client client,
        RabbitMqPublisher publisher,
        IConfiguration config)
    {
        _logger = logger;
        _client = client;
        _publisher = publisher;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Опрос Service3 запущен");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await PollOnceAsync();
                var interval = _config.GetValue<int>("Consumer:PollIntervalSeconds", 60);
                await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("PollingWorker остановлен");
        }
    }

    private async Task PollOnceAsync()
    {
        var keys = _config.GetSection("Consumer:ApiKeys").Get<List<string>>() ?? new();
        if (keys.Count == 0)
        {
            _logger.LogWarning("Нет API ключей в конфиге");
            return;
        }

        var apiKey = keys[_random.Next(keys.Count)];
        var correlationId = Guid.NewGuid().ToString();
        var count = _config.GetValue<int>("Consumer:ItemsPerRequest", 50);

        _logger.LogInformation("Опрашиваю Service3. CorrelationId={CorrelationId}", correlationId);

        try
        {
            var data = await _client.GetDataAsync(apiKey, count, correlationId);
            if (data != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(data, _jsonOptions);
                _publisher.Publish(json);
                _logger.LogInformation("Данные отправлены в очередь");
            }
        }
        catch (TokenLimitExceededException ex)
        {
            _logger.LogWarning("Превышен лимит токенов, сбрасываю ключ ...{Suffix}", ex.Key[^4..]);
            await _client.ResetKeyAsync(ex.Key);

            await Task.Delay(3000);

            var data = await _client.GetDataAsync(ex.Key, count, correlationId);
            if (data != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(data, _jsonOptions);
                _publisher.Publish(json);
            }
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogError("Ключ ...{Suffix} не авторизован", ex.Key[^4..]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при опросе Service3");
        }
    }
}