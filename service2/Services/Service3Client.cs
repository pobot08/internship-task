using System.Net.Http.Json;
using System.Text.Json;
using service2.DTOs;

namespace service2.Services;

public class Service3Client
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<Service3Client> _logger;
    private readonly IConfiguration _config;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public Service3Client(HttpClient httpClient, ILogger<Service3Client> logger, IConfiguration config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config;

        var service3Url = config["Consumer:Service3Url"] ?? "http://localhost:5003";
        _httpClient.BaseAddress = new Uri(service3Url);
    }

    public async Task<Service3Response?> GetDataAsync(string apiKey, int count, string correlationId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/data?count={count}");
        request.Headers.Add("X-Api-Key", apiKey);
        request.Headers.Add("X-Correlation-Id", correlationId);

        _logger.LogInformation("Запрос к Service3. Key=...{Suffix}", apiKey[^4..]);

        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            throw new TokenLimitExceededException(apiKey);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedException(apiKey);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Service3Response>(_jsonOptions);
    }

    public async Task ResetKeyAsync(string apiKey)
    {
        _logger.LogInformation("Сброс лимита для ключа ...{Suffix}", apiKey[^4..]);
        var response = await _httpClient.PostAsync($"/api/keys/{apiKey}/reset", null);
        response.EnsureSuccessStatusCode();
    }
}

public class TokenLimitExceededException(string key) : Exception($"Token limit exceeded for key {key}")
{
    public string Key { get; } = key;
}

public class UnauthorizedException(string key) : Exception($"Unauthorized for key {key}")
{
    public string Key { get; } = key;
}