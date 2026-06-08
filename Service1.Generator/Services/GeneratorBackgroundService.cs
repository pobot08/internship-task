using service1.Data;
using service1.Models;
using Microsoft.EntityFrameworkCore;

namespace service1.Services
{
    public class GeneratorBackgroundService : BackgroundService
    {
        private readonly DataGenerator _generator;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<GeneratorBackgroundService> _logger;
        private readonly RuntimeConfig _runtimeConfig;

        public GeneratorBackgroundService(
            DataGenerator generator,
            IServiceScopeFactory scopeFactory,
            ILogger<GeneratorBackgroundService> logger,
            RuntimeConfig runtimeConfig)
        {
            _generator = generator;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _runtimeConfig = runtimeConfig;
        }

        // этот метод запускается автоматически когда сервис стартует
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Генератор запущен");

            // крутимся пока не получим сигнал остановки
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await GenerateAndSaveAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка во время генерации батча");
                }

                _logger.LogInformation(
                    "Следующая генерация через {Seconds} секунд", _runtimeConfig.IntervalSeconds);

                // ждём нужное время или пока не придёт сигнал остановки
                await Task.Delay(
                    TimeSpan.FromSeconds(_runtimeConfig.IntervalSeconds), stoppingToken);
            }

            _logger.LogInformation("Генератор остановлен");
        }

        private async Task GenerateAndSaveAsync()
        {
            var minItems = _runtimeConfig.MinItems;
            var maxItems = _runtimeConfig.MaxItems;

            var batch = _generator.GenerateBatch(minItems, maxItems);

            _logger.LogInformation(
                "Сгенерирован батч {BatchId} с {Count} объектами",
                batch.BatchId, batch.ItemsCount);

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Batches.Add(batch);
            await db.SaveChangesAsync();

            _logger.LogInformation("Батч {BatchId} сохранён в БД", batch.BatchId);
        }
    }
}