using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using service2.Data;

namespace service2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BatchesController : ControllerBase
{
    private readonly AppDbContext _db;

    public BatchesController(AppDbContext db) => _db = db;

    // GET /api/batches?page=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var total = await _db.Batches.CountAsync();
        var items = await _db.Batches
            .OrderByDescending(b => b.ReceivedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new { b.Id, b.SourceBatchId, b.TransformedAt, b.ItemsCount, b.ReceivedAt })
            .ToListAsync();

        return Ok(new { page, pageSize, totalCount = total, items });
    }

    // GET /api/batches/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var batch = await _db.Batches
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == id);

        return batch == null ? NotFound() : Ok(batch);
    }

    // GET /api/batches/5/file
    [HttpGet("{id}/file")]
    public async Task<IActionResult> DownloadFile(int id)
    {
        var batch = await _db.Batches.FindAsync(id);

        if (batch == null)
            return NotFound("Батч не найден");

        if (string.IsNullOrEmpty(batch.ExcelFilePath) || !System.IO.File.Exists(batch.ExcelFilePath))
            return NotFound("Excel файл ещё не создан");

        var bytes = await System.IO.File.ReadAllBytesAsync(batch.ExcelFilePath);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            Path.GetFileName(batch.ExcelFilePath));
    }
}