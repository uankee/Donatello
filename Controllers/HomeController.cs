using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Donatello.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        Response.Cookies.Append("DonatelloDemoCookie", "TestValue", new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTimeOffset.UtcNow.AddDays(1)
        });

        return RedirectToAction("Index", "Boards");
    }

    public IActionResult About() => View();
}
