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

builder.Services.AddHttpClient<IService1Client, Service1Client>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Service1:BaseUrl"]!);
});

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var connectionString =
     builder.Configuration["Redis:ConnectionString"]
     ?? throw new InvalidOperationException(
         "Redis connection string not configured");

    return ConnectionMultiplexer.Connect(connectionString);
});

builder.Services.AddSingleton<ITokenStore, RedisTokenStore>();

builder.Services.AddScoped<ITransformationService, TransformationService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.MapHealthChecks("/health");
app.MapHealthChecks("/ready");

app.Run();