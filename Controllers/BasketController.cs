using KitapProject.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Oturum açma durumunu kontrol etmek için bu kütüphaneyi ekleyin

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

        /// <summary>
        /// Kullanıcının oturum açmış olup olmadığını kontrol eder ve JSON olarak yanıt döner.
        /// </summary>
        [HttpGet] // Bu action'a HTTP GET isteği ile erişilecek
        public IActionResult CheckLoginStatus()
        {
            // User.Identity.IsAuthenticated, ASP.NET Core'da kullanıcının oturum açıp açmadığını kontrol eder.
            // Bu, ASP.NET Core Identity veya başka bir kimlik doğrulama sistemi kullanıyorsanız çalışır.
            bool isLoggedIn = User.Identity.IsAuthenticated;

            // JSON formatında bir yanıt döndürüyoruz.
            return Json(new { isLoggedIn = isLoggedIn });
        }
    }

    // --- Aşağıdaki kısımlar, Order/Index ve Account/Login sayfalarınızın bulunduğu Controller'lar için örnektir.
    // --- Eğer bu controller'lar projenizde zaten varsa, bu kodları eklemenize gerek yoktur.

    // Örnek: Sipariş sayfanızın bulunduğu Controller

}