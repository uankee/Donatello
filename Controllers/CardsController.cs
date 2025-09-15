using Donatello.Data;
using Donatello.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Donatello.Controllers
{
    public class CardsController : Controller
    {
        private readonly DonatelloDbContext ctx;
        private readonly IWebHostEnvironment env;

        public CardsController(DonatelloDbContext context, IWebHostEnvironment environment)
        {
            ctx = context;
            env = environment;
        }

        [HttpGet]
        public IActionResult Create(int columnId)
        {
            ViewBag.ColumnId = columnId;
            return View();
        }

        [HttpPost]
        public IActionResult Create(Card card, IFormFile? attachment)
        {
            if (attachment != null)
            {
                var uploads = Path.Combine(env.WebRootPath, "uploads");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                var filePath = Path.Combine(uploads, attachment.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    attachment.CopyTo(stream);
                }
                card.AttachmentPath = "/uploads/" + attachment.FileName;
            }

            ctx.Cards.Add(card);
            ctx.SaveChanges();
            var column = ctx.Columns.Find(card.ColumnId);
            return RedirectToAction("Details", "Boards", new { id = column!.BoardId });
        }
    }
}
