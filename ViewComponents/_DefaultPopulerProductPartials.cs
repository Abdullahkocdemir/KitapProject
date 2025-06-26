using KitapProject.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitapProject.ViewComponents
{
    public class _DefaultPopulerProductPartials : ViewComponent
    {
        private readonly BookContext _context;

        public _DefaultPopulerProductPartials(BookContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var popularProducts = _context.Products
                                          .Where(p => p.PopulerProduct && p.Status)
                                          .Include(p => p.Category)
                                          .Where(y => y.PopulerProduct == true) 
                                          .ToList();

            return View(popularProducts);
        }
    }
}
