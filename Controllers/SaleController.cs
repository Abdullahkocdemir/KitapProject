using KitapProject.Context;
using KitapProject.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitapProject.Controllers
{
    [Authorize] 
    public class SaleController : Controller
    {
        private readonly BookContext _context;
        private readonly UserManager<AppUser> _userManager;

        public SaleController(BookContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }

            var userOrders = await _context.Orders
                .Where(o => o.AppUserId == currentUserId)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(userOrders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var currentUserId = _userManager.GetUserId(User);

            var order = await _context.Orders
                .Where(o => o.OrderId == id && o.AppUserId == currentUserId) 
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.AppUser)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound("Sipariş bulunamadı veya erişim yetkiniz yok.");
            }

            return View(order);
        }
    }
}