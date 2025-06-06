using Microsoft.AspNetCore.Mvc;

namespace KitapProject.ViewComponents
{
    public class _AdminScriptPartials : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
