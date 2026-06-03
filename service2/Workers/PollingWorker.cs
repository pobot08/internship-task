using DocumentFormat.OpenXml.Bibliography;
using service2.Services;


namespace service2.Workers
{
    public class PollingWorker : BackgroundService
    {

        private readonly ILogger<PollingWorker> _logger;
        private readonly Service3Client _client;
        private readonly RabbitMqPublisher _publisher;

        public PollingWorker(ILogger<PollingWorker> logger, Service3Client client, RabbitMqPublisher publisher)
        {
            _logger = logger;
            _client = client;
            _publisher = publisher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Опрос Service3 запущен");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Делаю запрос к Service3...");

                    var data = await _client.GetDataAsync();
                    _publisher.Publish(data);

                    _logger.LogInformation("Данные отправлены в очередь");

                    await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("PollingWorker остановлен");
            }
        }
    }
}
