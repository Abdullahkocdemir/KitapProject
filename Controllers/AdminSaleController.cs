// KitapProject.Controllers/AdminSaleController.cs
using KitapProject.Context;
using KitapProject.Entities;
using KitapProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace KitapProject.Controllers
{
    public class AdminSaleController : Controller
    {
        private readonly BookContext _context;

        public AdminSaleController(BookContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                                       .Include(o => o.AppUser)
                                       .OrderByDescending(o => o.OrderDate)
                                       .ToListAsync();
            return View(orders);
        }
        [HttpPost("/AdminSale/UpdateOrderStatus")] 
        public async Task<IActionResult> UpdateOrderStatus([FromBody] OrderStatusUpdateModel model) 
        {
            var order = await _context.Orders.FindAsync(model.OrderId);

            if (order == null)
            {
                return NotFound("Sipariş bulunamadı."); 
            }

            if (Enum.TryParse(typeof(OrderStatus), model.NewStatus, true, out var parsedStatus))
            {
                order.OrderStatus = parsedStatus.ToString()!;
                _context.Update(order);
                await _context.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return BadRequest("Geçersiz sipariş durumu.");
            }
        }
    }
}

