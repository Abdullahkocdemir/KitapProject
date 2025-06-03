using KitapProject.Context;
using Microsoft.AspNetCore.Mvc;

namespace KitapProject.Controllers
{
    public class BasketController : Controller
    {
        private readonly BookContext _context;

        public BasketController(BookContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
