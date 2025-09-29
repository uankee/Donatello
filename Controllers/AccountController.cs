using Donatello.Models;
using Donatello.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Donatello.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Register: ModelState invalid. Errors: {@ModelStateErrors}", ModelState);
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with email: {Email}", model.Email);

                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Boards");
            }

            foreach (var error in result.Errors)
            {
                _logger.LogWarning("Register Identity error: {Code} - {Description}", error.Code, error.Description);
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Login: ModelState invalid. Errors: {@ModelStateErrors}", ModelState);
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in: {Email}", model.Email);
                return RedirectToAction("Index", "Boards");
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Account locked out.");
            }
            else
            {
                _logger.LogInformation("Invalid login attempt for {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied() => View();
    }
}
