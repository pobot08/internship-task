using Microsoft.AspNetCore.Mvc;
using Service3.Proxy.Services;

namespace Service3.Proxy.Controllers;

[ApiController]
[Route("api/keys")]
public class KeysController : ControllerBase
{
    private readonly ITokenStore _tokenStore;

    public KeysController(
        ITokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    [HttpGet("{key}/usage")]
    public IActionResult Usage(string key)
    {
        return Ok(new
        {
            key,
            used = _tokenStore.GetUsed(key)
        });
    }

    [HttpPost("{key}/reset")]
    public async Task<IActionResult> Reset(string key)
    {
        await Task.Delay(
            Random.Shared.Next(2000, 5000));

        _tokenStore.Reset(key);

        return Ok(new
        {
            status = "reset",
            key
        });
    }
}