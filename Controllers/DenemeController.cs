using Microsoft.AspNetCore.Mvc;

namespace KitapProject.Controllers
{
    public class DenemeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
