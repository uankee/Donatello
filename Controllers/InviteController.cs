using Donatello.Data;
using Donatello.Data.Entities;
using Donatello.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Donatello.Controllers
{
    [Authorize]
    public class InviteController : Controller
    {
        private readonly DonatelloDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InviteController(DonatelloDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult InviteUser(int boardId)
        {
            return View(new InviteViewModel { BoardId = boardId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteUser(InviteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Невірні дані.";
                return RedirectToAction("Details", "Boards", new { id = model.BoardId });
            }

            var board = await _context.Boards
                .Include(b => b.BoardUsers)
                .ThenInclude(bu => bu.User)
                .FirstOrDefaultAsync(b => b.Id == model.BoardId);

            if (board == null)
            {
                TempData["ErrorMessage"] = "Дошка не знайдена.";
                return RedirectToAction("Index", "Boards");
            }

            var inviter = await _userManager.GetUserAsync(User);
            if (inviter == null)
            {
                TempData["ErrorMessage"] = "Потрібно увійти в систему.";
                return RedirectToAction("Login", "Account");
            }

            var inviterRole = board.BoardUsers.FirstOrDefault(bu => bu.UserId == inviter.Id)?.Role;
            if (inviterRole != "Admin")
            {
                TempData["ErrorMessage"] = "Лише адміністратор може запрошувати користувачів.";
                return RedirectToAction("Details", "Boards", new { id = model.BoardId });
            }

            var userToInvite = await _userManager.FindByEmailAsync(model.Email);
            if (userToInvite == null)
            {
                TempData["ErrorMessage"] = "Користувача з таким email не знайдено. Він має зареєструватися спочатку.";
                return RedirectToAction("Details", "Boards", new { id = model.BoardId });
            }

            if (board.BoardUsers.Any(bu => bu.UserId == userToInvite.Id))
            {
                TempData["ErrorMessage"] = "Користувач вже має доступ до цієї дошки.";
                return RedirectToAction("Details", "Boards", new { id = model.BoardId });
            }

            var boardUser = new BoardUser
            {
                BoardId = board.Id,
                UserId = userToInvite.Id,
                Role = "User" 
            };

            _context.BoardUsers.Add(boardUser);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Користувач {userToInvite.Email} успішно доданий до дошки.";
            return RedirectToAction("Details", "Boards", new { id = model.BoardId });
        }
    }
}
