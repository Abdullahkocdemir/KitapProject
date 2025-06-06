using Microsoft.AspNetCore.Mvc;

namespace KitapProject.ViewComponents
{
    public class _DefaultCountNumberPartials : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
