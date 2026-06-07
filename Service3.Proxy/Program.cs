using Microsoft.Extensions.Options;
using Service3.Proxy.Models;
using Service3.Proxy.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<List<ApiKeyConfig>>(
    builder.Configuration.GetSection("ApiKeys"));

// Типизированный клиент к Service1
builder.Services.AddHttpClient<IService1Client, Service1Client>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Service1:BaseUrl"]!);
});

// Отдельный именованный клиент для health-check /ready
builder.Services.AddHttpClient("service1-health", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Service1:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(3);
});

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var connectionString =
        builder.Configuration["Redis:ConnectionString"]
        ?? throw new InvalidOperationException("Redis connection string not configured");

    return ConnectionMultiplexer.Connect(connectionString);
});

builder.Services.AddSingleton<ITokenStore, RedisTokenStore>();
builder.Services.AddScoped<ITransformationService, TransformationService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

// Liveness — сервис просто живой
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

// Readiness — проверяем Redis и доступность Service1
app.MapGet("/ready", async (IHttpClientFactory httpClientFactory, IConnectionMultiplexer redis) =>
{
    var issues = new List<string>();

    // Проверяем Redis
    try
    {
        await redis.GetDatabase().PingAsync();
    }
    catch (Exception ex)
    {
        issues.Add($"redis: {ex.Message}");
    }

    // Проверяем Service1
    try
    {
        var client = httpClientFactory.CreateClient("service1-health");
        var response = await client.GetAsync("/health");
        if (!response.IsSuccessStatusCode)
            issues.Add($"service1: responded with {(int)response.StatusCode}");
    }
    catch (Exception ex)
    {
        issues.Add($"service1: {ex.Message}");
    }

    return issues.Count == 0
        ? Results.Ok(new { status = "ready" })
        : Results.Json(new { status = "not ready", issues }, statusCode: 503);
});

app.Run();