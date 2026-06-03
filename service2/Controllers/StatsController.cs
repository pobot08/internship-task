using Microsoft.AspNetCore.Mvc;
using service2.Data;

namespace service2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly AppDbContext _db;

    public StatsController(AppDbContext db) => _db = db;

    // GET /api/stats
    [HttpGet]
    public IActionResult Get()
    {
        var totalBatches = _db.Batches.Count();
        var totalItems = _db.Items.Count();
        var lastBatch = _db.Batches
            .OrderByDescending(b => b.ReceivedAt)
            .FirstOrDefault();

        return Ok(new
        {
            totalBatches,
            totalItems,
            lastPollAt = lastBatch?.ReceivedAt
        });
    }
}