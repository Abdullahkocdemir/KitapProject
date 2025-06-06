using KitapProject.Context;
using Microsoft.AspNetCore.Mvc;

namespace KitapProject.ViewComponents
{
    public class _DefaultPopulerCategoryPartials : ViewComponent
    {
        private readonly BookContext _context;

        public _DefaultPopulerCategoryPartials(BookContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var values = _context.Categories.ToList();
            return View(values);
        }
    }
}
