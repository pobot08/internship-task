using Microsoft.AspNetCore.Mvc;
using service1.Models;

namespace service1.Controllers
{
    [ApiController]
    [Route("api/config")]
    public class ConfigController : ControllerBase
    {
        // IConfiguration — только для чтения
        // чтобы менять настройки на лету нам нужен отдельный объект
        // который живёт всё время работы сервиса — поэтому Singleton
        private readonly RuntimeConfig _runtimeConfig;

        public ConfigController(RuntimeConfig runtimeConfig)
        {
            _runtimeConfig = runtimeConfig;
        }

        // GET /api/config
        [HttpGet]
        public IActionResult GetConfig()
        {
            return Ok(new GeneratorConfig
            {
                IntervalSeconds = _runtimeConfig.IntervalSeconds,
                MinItems = _runtimeConfig.MinItems,
                MaxItems = _runtimeConfig.MaxItems
            });
        }

        // PUT /api/config
        [HttpPut]
        public IActionResult UpdateConfig([FromBody] GeneratorConfig config)
        {
            // валидация — проверяем что значения разумные
            if (config.IntervalSeconds < 1)
                return BadRequest(new { message = "IntervalSeconds должен быть больше 0" });

            if (config.MinItems < 1)
                return BadRequest(new { message = "MinItems должен быть больше 0" });

            if (config.MaxItems < config.MinItems)
                return BadRequest(new { message = "MaxItems должен быть >= MinItems" });

            // обновляем настройки — они сразу применятся в BackgroundService
            _runtimeConfig.IntervalSeconds = config.IntervalSeconds;
            _runtimeConfig.MinItems = config.MinItems;
            _runtimeConfig.MaxItems = config.MaxItems;

            return Ok(new
            {
                message = "Конфигурация обновлена",
                config
            });
        }
    }
}