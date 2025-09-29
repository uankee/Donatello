using Donatello.Data;
using Donatello.Data.Entities;
using Donatello.Extensions;
using Donatello.Models;
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
            .Where(b => b.BoardUsers.Any(bu => bu.UserId == user.Id))
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

        _db.Boards.Add(board);
        await _db.SaveChangesAsync();

        // робимо творця дошки Admin
        var boardUser = new BoardUser
        {
            BoardId = board.Id,
            UserId = user.Id,
            Role = "Admin"
        };
        _db.BoardUsers.Add(boardUser);
        await _db.SaveChangesAsync();

        TempData.Set("ToastMessage", new ToastModel
        {
            Message = $"Дошку \"{board.Title}\" успішно створено!",
            Type = ToastType.success
        });

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var board = await _db.Boards
            .Include(b => b.BoardUsers)
            .FirstOrDefaultAsync(b => b.Id == id && b.BoardUsers.Any(bu => bu.UserId == user.Id));

        if (board == null) return Forbid();
        return View(board);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Board board)
    {
        var user = await _userManager.GetUserAsync(User);
        var exists = await _db.Boards
            .Include(b => b.BoardUsers)
            .AnyAsync(b => b.Id == board.Id && b.BoardUsers.Any(bu => bu.UserId == user.Id));

        if (!exists) return Forbid();
        if (!ModelState.IsValid) return View(board);

        _db.Boards.Update(board);
        await _db.SaveChangesAsync();

        TempData.Set("ToastMessage", new ToastModel
        {
            Message = $"Дошку \"{board.Title}\" оновлено!",
            Type = ToastType.info
        });

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            var boardUser = await _db.BoardUsers
                .FirstOrDefaultAsync(bu => bu.BoardId == id && bu.UserId == user.Id);

            if (boardUser == null || boardUser.Role != "Admin") return Forbid();

            var board = await _db.Boards
                .Include(b => b.Columns)
                .ThenInclude(c => c.Cards)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (board == null) return NotFound();

            _db.Cards.RemoveRange(board.Columns.SelectMany(c => c.Cards));
            _db.Columns.RemoveRange(board.Columns);
            _db.BoardUsers.RemoveRange(_db.BoardUsers.Where(bu => bu.BoardId == id));
            _db.Boards.Remove(board);

            await _db.SaveChangesAsync();

            TempData.Set("ToastMessage", new ToastModel
            {
                Message = $"Дошку \"{board.Title}\" видалено!",
                Type = ToastType.warning
            });

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting board id={BoardId}", id);

            TempData.Set("ToastMessage", new ToastModel
            {
                Message = "Під час видалення дошки сталася помилка.",
                Type = ToastType.danger
            });

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
            .Include(b => b.BoardUsers)
                .ThenInclude(bu => bu.User)
            .AsSplitQuery()
            .FirstOrDefaultAsync(b => b.Id == id && b.BoardUsers.Any(bu => bu.UserId == user.Id));

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
                .Where(b => boardIds.Contains(b.Id) && b.BoardUsers.Any(bu => bu.UserId == user.Id))
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
