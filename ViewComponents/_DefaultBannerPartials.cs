using Microsoft.AspNetCore.Mvc;

namespace KitapProject.ViewComponents
{
    public class _DefaultBannerPartials : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
