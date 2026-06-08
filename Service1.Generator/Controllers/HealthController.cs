using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using service1.Data;

namespace service1.Controllers
{
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly AppDbContext _db;

        public HealthController(AppDbContext db)
        {
            _db = db;
        }

        // GET /health
        [HttpGet("/health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy" });
        }

        // GET /ready
        [HttpGet("/ready")]
        public async Task<IActionResult> Ready()
        {
            try
            {
                await _db.Database.ExecuteSqlRawAsync("SELECT 1");
                return Ok(new { status = "ready", database = "connected" });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    status = "not ready",
                    database = "unavailable",
                    error = ex.Message
                });
            }
        }
    }
}