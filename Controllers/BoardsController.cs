using Donatello.Data;
using Donatello.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Donatello.Controllers;

public class BoardsController : Controller
{
    private readonly DonatelloDbContext _db;
    private readonly ILogger<BoardsController> _logger;

    public BoardsController(DonatelloDbContext db, ILogger<BoardsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var boards = _db.Boards
            .AsNoTracking()
            .OrderBy(b => b.Order)
            .ToList();
        return View(boards);
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Board board)
    {
        if (!ModelState.IsValid) return View(board);

        var lastOrder = _db.Boards.OrderByDescending(b => b.Order).Select(b => (int?)b.Order).FirstOrDefault();
        board.Order = (lastOrder ?? -1) + 1;

        _db.Boards.Add(board);
        _db.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var board = _db.Boards.Find(id);
        if (board == null) return NotFound();
        return View(board);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Board board)
    {
        if (!ModelState.IsValid) return View(board);
        _db.Boards.Update(board);
        _db.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        try
        {
            var board = _db.Boards.Find(id);
            if (board == null) return NotFound();

            var columns = _db.Columns.Where(c => c.BoardId == id).ToList();
            if (columns.Any())
            {
                var columnIds = columns.Select(c => c.Id).ToList();
                var cards = _db.Cards.Where(card => columnIds.Contains(card.ColumnId)).ToList();
                if (cards.Any())
                {
                    _db.Cards.RemoveRange(cards);
                }
                _db.Columns.RemoveRange(columns);
            }

            _db.Boards.Remove(board);
            _db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting board id={BoardId}", id);
            TempData["ErrorMessage"] = "Під час видалення дошки сталася помилка. Деталі в логах.";
            return RedirectToAction(nameof(Index));
        }
    }

    public IActionResult Details(int id)
    {
        var board = _db.Boards
            .Include(b => b.Columns)
                .ThenInclude(c => c.Cards)
            .AsSplitQuery()
            .FirstOrDefault(b => b.Id == id);

        if (board == null) return NotFound();

        board.Columns = board.Columns.OrderBy(c => c.Order).ToList();
        foreach (var column in board.Columns)
        {
            column.Cards = column.Cards.OrderBy(c => c.Order).ToList();
        }

        return View(board);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reorder([FromBody] List<int> boardIds)
    {
        if (boardIds == null || !boardIds.Any())
            return BadRequest("boardIds required");

        try
        {
            var boards = await _db.Boards.Where(b => boardIds.Contains(b.Id)).ToListAsync();

            for (int i = 0; i < boardIds.Count; i++)
            {
                var id = boardIds[i];
                var b = boards.FirstOrDefault(x => x.Id == id);
                if (b != null)
                {
                    b.Order = i;
                }
            }

            await _db.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering boards");
            return StatusCode(500, "Error saving order");
        }
    }
}
