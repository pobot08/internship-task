using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Service3.Proxy.Models;
using Service3.Proxy.Services;

namespace Service3.Proxy.Controllers;

[ApiController]
[Route("api/data")]
public class DataController : ControllerBase
{
    private readonly IService1Client _service1Client;
    private readonly ITransformationService _transformer;
    private readonly ITokenStore _tokenStore;
    private readonly List<ApiKeyConfig> _apiKeys;

    public DataController(
        IService1Client service1Client,
        ITransformationService transformer,
        ITokenStore tokenStore,
        IOptions<List<ApiKeyConfig>> apiKeys)
    {
        _service1Client = service1Client;
        _transformer = transformer;
        _tokenStore = tokenStore;
        _apiKeys = apiKeys.Value;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int count = 10,
        [FromHeader(Name = "X-Api-Key")]
        string apiKey = "")
    {
        var keyConfig =
            _apiKeys.FirstOrDefault(x => x.Key == apiKey);

        if (keyConfig == null)
        {
            return Unauthorized();
        }

        var sourceItems =
            await _service1Client.GetLatestItemsAsync(count);

        var transformed =
            _transformer.Transform(sourceItems);

        var tokens =
            _transformer.CountTokens(transformed);

        bool success = _tokenStore.TryAdd(
            apiKey,
            keyConfig.TokenLimit,
            tokens);

        if (!success)
        {
            return StatusCode(429, new
            {
                error = "TOKEN_LIMIT_EXCEEDED",
                tokens_limit = keyConfig.TokenLimit,
                tokens_used = _tokenStore.GetUsed(apiKey)
            });
        }

        var response = new EnvelopeResponse
        {
            Envelope = new Envelope
            {
                SourceBatchId = Guid.NewGuid().ToString(),
                TransformedAt = DateTime.UtcNow,
                ItemsCount = transformed.Count,
                TokensUsed = tokens,
                TokensRemaining =
                    keyConfig.TokenLimit -
                    _tokenStore.GetUsed(apiKey)
            },

            Items = transformed
        };

        return Ok(response);
    }
}