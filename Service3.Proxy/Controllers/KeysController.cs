using Microsoft.AspNetCore.Mvc;

namespace Service3.Proxy.Controllers;

[ApiController]
[Route("[controller]")]
public class KeysController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<KeysController> _logger;

    public KeysController(ILogger<KeysController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<KeysController> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new KeysController
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
