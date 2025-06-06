using Microsoft.AspNetCore.Mvc;

namespace KitapProject.ViewComponents
{
    public class _DefaultMembershipPartials : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
