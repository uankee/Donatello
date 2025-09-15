using Microsoft.AspNetCore.Mvc;

namespace Donatello.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Boards");
    }

    public IActionResult About() => View();
}
