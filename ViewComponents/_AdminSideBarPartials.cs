using Microsoft.AspNetCore.Mvc;

namespace KitapProject.ViewComponents
{
    public class _AdminSideBarPartials : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
