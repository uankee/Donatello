using Donatello.Data;
using Donatello.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Donatello.Controllers;

[Authorize]
public class BoardsController : Controller
{
    private readonly DonatelloDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<BoardsController> _logger;

    public BoardsController(DonatelloDbContext db, UserManager<ApplicationUser> userManager, ILogger<BoardsController> logger)
    {
        _db = db;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var boards = await _db.Boards
            .Where(b => b.Users.Any(u => u.Id == user.Id))
            .OrderBy(b => b.Order)
            .AsNoTracking()
            .ToListAsync();
        return View(boards);
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Board board)
    {
        if (!ModelState.IsValid) return View(board);

        var lastOrder = await _db.Boards.MaxAsync(b => (int?)b.Order) ?? -1;
        board.Order = lastOrder + 1;

        var user = await _userManager.GetUserAsync(User);
        board.Users.Add(user);

        _db.Boards.Add(board);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var board = await _db.Boards
            .Include(b => b.Users)
            .FirstOrDefaultAsync(b => b.Id == id && b.Users.Any(u => u.Id == user.Id));

        if (board == null) return Forbid();
        return View(board);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Board board)
    {
        var user = await _userManager.GetUserAsync(User);
        var exists = await _db.Boards
            .Include(b => b.Users)
            .AnyAsync(b => b.Id == board.Id && b.Users.Any(u => u.Id == user.Id));

        if (!exists) return Forbid();
        if (!ModelState.IsValid) return View(board);

        _db.Boards.Update(board);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            var board = await _db.Boards
                .Include(b => b.Users)
                .FirstOrDefaultAsync(b => b.Id == id && b.Users.Any(u => u.Id == user.Id));

            if (board == null) return Forbid();

            var columns = await _db.Columns.Where(c => c.BoardId == id).ToListAsync();
            if (columns.Any())
            {
                var columnIds = columns.Select(c => c.Id).ToList();
                var cards = await _db.Cards.Where(c => columnIds.Contains(c.ColumnId)).ToListAsync();
                if (cards.Any()) _db.Cards.RemoveRange(cards);
                _db.Columns.RemoveRange(columns);
            }

            _db.Boards.Remove(board);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting board id={BoardId}", id);
            TempData["ErrorMessage"] = "Під час видалення дошки сталася помилка. Деталі в логах.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var board = await _db.Boards
            .Include(b => b.Columns)
                .ThenInclude(c => c.Cards)
            .Include(b => b.Users)
            .AsSplitQuery()
            .FirstOrDefaultAsync(b => b.Id == id && b.Users.Any(u => u.Id == user.Id));

        if (board == null) return Forbid();

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
            var user = await _userManager.GetUserAsync(User);
            var boards = await _db.Boards
                .Where(b => boardIds.Contains(b.Id) && b.Users.Any(u => u.Id == user.Id))
                .ToListAsync();

            for (int i = 0; i < boardIds.Count; i++)
            {
                var id = boardIds[i];
                var b = boards.FirstOrDefault(x => x.Id == id);
                if (b != null) b.Order = i;
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
