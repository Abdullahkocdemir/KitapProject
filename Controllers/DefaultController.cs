using Microsoft.AspNetCore.Mvc;
using System.Linq;
using KitapProject.Entities;
using Microsoft.EntityFrameworkCore;
using KitapProject.Context;

namespace KitapProject.Controllers
{
    public class DefaultController : Controller
    {
        private readonly BookContext _context;

        public DefaultController(BookContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var popularProducts = _context.Products
                                          .Where(p => p.PopulerProduct && p.Status)
                                          .Include(p => p.Category)
                                          .Where(y => y.PopulerProduct == true) // Bu ikinci Where koşulu ilkini tekrar ediyor, istersen kaldırabilirsin.
                                          .ToList();

            return View(popularProducts);
        }
    }
}