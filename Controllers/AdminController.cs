using KitapProject.Context;
using KitapProject.Entities; // AppUser ve AppRole için
using Microsoft.AspNetCore.Identity; // UserManager ve RoleManager için
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Include ve ToListAsync için

namespace KitapProject.Controllers
{
    public class AdminController : Controller
    {
        private readonly BookContext _context;
        private readonly UserManager<AppUser> _userManager; // Kullanıcı yönetimi için
        private readonly RoleManager<AppRole> _roleManager; // Rol yönetimi için

        public AdminController(BookContext context, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // --- Kullanıcıları Listeleme (Index) ---
        public async Task<IActionResult> Index()
        {
            // Tüm kullanıcıları ve rolleriyle birlikte getiriyoruz.
            // Bu, her bir kullanıcının hangi rollere sahip olduğunu görmemizi sağlayacak.
            var users = await _userManager.Users.ToListAsync();

            // Kullanıcıların rollerini tutacak bir ViewModel oluşturabilirsiniz
            // Ancak şimdilik View'da dinamik olarak rolleri çekeceğiz.
            // Daha performanslı bir çözüm için ViewModel kullanmak daha iyi olabilir.

            return View(users);
        }

        // --- Kullanıcı Detaylarını Görüntüleme ---
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Kullanıcının sahip olduğu rolleri alalım
            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRoles = userRoles; // View'a rolleri göndermek için

            return View(user);
        }

        // --- Kullanıcı Silme (HTTP POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF saldırılarına karşı koruma
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return Json(new { success = false, message = "Geçersiz kullanıcı ID'si." });
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return Json(new { success = true, message = "Kullanıcı başarıyla silindi." });
            }
            else
            {
                // Silme hatası durumunda detayları döndür
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Json(new { success = false, message = $"Kullanıcı silinirken bir hata oluştu: {errors}" });
            }
        }
    }
}