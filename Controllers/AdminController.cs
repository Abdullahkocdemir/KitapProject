using KitapProject.Context;
using KitapProject.Entities; 
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 

namespace KitapProject.Controllers
{
    public class AdminController : Controller
    {
        private readonly BookContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager; 

        public AdminController(BookContext context, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

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

            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRoles = userRoles; 

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
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
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Json(new { success = false, message = $"Kullanıcı silinirken bir hata oluştu: {errors}" });
            }
        }
    }
}