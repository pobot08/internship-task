using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using service1.Data;
using service1.Models;

namespace service1.Controllers
{
    [ApiController]
    [Route("api/items")]
    public class ItemsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ItemsController(AppDbContext db)
        {
            _db = db;
        }

        // GET /api/items/latest?count=N
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest([FromQuery] int count = 10)
        {
            if (count < 1) count = 1;
            if (count > 1000) count = 1000;

            // загружаем последние батчи из БД
            var batches = await _db.Batches
                .OrderByDescending(b => b.GeneratedAt)
                .Take(50) // берём последние 50 батчей с запасом
                .ToListAsync();

            // из батчей достаём Items, объединяем в один список
            // SelectMany — разворачивает список списков в один список
            var items = batches
                .SelectMany(b => b.Items)
                .OrderByDescending(i => i.DataValue)
                .Take(count)
                .ToList();

            return Ok(new
            {
                count = items.Count,
                items
            });
        }
    }
}