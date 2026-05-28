using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using service1.Data;

namespace service1.Controllers
{
    [ApiController]
    [Route("api/batches")]
    public class BatchesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public BatchesController(AppDbContext db)
        {
            _db = db;
        }

        // GET /api/batches?page=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> GetBatches(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // защита от некорректных значений
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 1;
            if (pageSize > 100) pageSize = 100;

            // считаем общее количество батчей в БД
            var totalCount = await _db.Batches.CountAsync();

            // получаем только нужную страницу — без Items (только метаданные)
            // Skip — пропустить первые N записей
            // Take — взять следующие N записей
            var batches = await _db.Batches
                .OrderByDescending(b => b.GeneratedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new
                {
                    b.BatchId,
                    b.GeneratedAt,
                    b.ItemsCount
                })
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalCount,
                items = batches
            });
        }

        // GET /api/batches/{batchId}/items
        [HttpGet("{batchId}/items")]
        public async Task<IActionResult> GetBatchItems(Guid batchId)
        {
            var batch = await _db.Batches.FirstOrDefaultAsync(b => b.BatchId == batchId);

            // если батч не найден — возвращаем 404
            if (batch == null)
                return NotFound(new { message = $"Батч {batchId} не найден" });

            return Ok(batch.Items);
        }
    }
}