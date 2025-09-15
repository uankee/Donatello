using Donatello.Data;
using Donatello.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Donatello.Controllers;

public class ColumnsController : Controller
{
    private readonly DonatelloDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ColumnsController> _logger;

    public ColumnsController(DonatelloDbContext db, IWebHostEnvironment env, ILogger<ColumnsController> logger)
    {
        _db = db;
        _env = env;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Create(int boardId)
    {
        var board = await _db.Boards.AsNoTracking().FirstOrDefaultAsync(b => b.Id == boardId);
        if (board == null) return NotFound();

        ViewBag.BoardId = boardId;
        ViewBag.BoardTitle = board.Title;

        return View(new Column { BoardId = boardId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("BoardId,Title,Description,Color")] Column column, IFormFile? attachment)
    {
        if (string.IsNullOrWhiteSpace(column.Title))
            ModelState.AddModelError(nameof(column.Title), "Title is required");

        if (!ModelState.IsValid)
        {
            var board = await _db.Boards.AsNoTracking().FirstOrDefaultAsync(b => b.Id == column.BoardId);
            ViewBag.BoardId = column.BoardId;
            ViewBag.BoardTitle = board?.Title ?? "";
            ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return View(column);
        }

        try
        {
            var lastOrder = await _db.Columns
                .Where(c => c.BoardId == column.BoardId)
                .OrderByDescending(c => c.Order)
                .Select(c => (int?)c.Order)
                .FirstOrDefaultAsync();

            column.Order = (lastOrder ?? -1) + 1;

            if (attachment != null && attachment.Length > 0)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "columns");
                if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                var safeFileName = $"{Guid.NewGuid()}{Path.GetExtension(attachment.FileName)}";
                var filePath = Path.Combine(uploadsDir, safeFileName);

                using var fs = new FileStream(filePath, FileMode.Create);
                await attachment.CopyToAsync(fs);

                column.AttachmentPath = $"/uploads/columns/{safeFileName}";
            }

            _db.Columns.Add(column);
            await _db.SaveChangesAsync();

            return RedirectToAction("Details", "Boards", new { id = column.BoardId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating column for boardId={BoardId}", column.BoardId);
            ModelState.AddModelError(string.Empty, "Щось пішло не так при збереженні колонки. Перевір логи.");
            var board = await _db.Boards.AsNoTracking().FirstOrDefaultAsync(b => b.Id == column.BoardId);
            ViewBag.BoardId = column.BoardId;
            ViewBag.BoardTitle = board?.Title ?? "";
            ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return View(column);
        }
    }


    public class ReorderRequest
    {
        public int BoardId { get; set; }
        public List<int> ColumnIds { get; set; } = new List<int>();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reorder([FromBody] ReorderRequest req)
    {
        if (req == null || req.ColumnIds == null || !req.ColumnIds.Any())
            return BadRequest("Invalid request");

        try
        {
            var columns = await _db.Columns.Where(c => c.BoardId == req.BoardId && req.ColumnIds.Contains(c.Id)).ToListAsync();

            for (int i = 0; i < req.ColumnIds.Count; i++)
            {
                var id = req.ColumnIds[i];
                var col = columns.FirstOrDefault(c => c.Id == id);
                if (col != null)
                {
                    col.Order = i;
                }
            }

            await _db.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering columns for boardId={BoardId}", req?.BoardId);
            return StatusCode(500, "Error saving column order");
        }
    }
}
