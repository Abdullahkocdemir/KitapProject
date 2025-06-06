// KitapProject.Controllers/AdminSaleController.cs
using KitapProject.Context;
using KitapProject.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace KitapProject.Controllers
{
    // İsteğe bağlı olarak controller seviyesinde bir route tanımlayabilirsiniz
    // [Route("[controller]")] // Bu satırı eklerseniz, URL "/AdminSale/UpdateOrderStatus" yerine "/AdminSale/UpdateOrderStatus" çalışır.
    // Eğer bu satırı eklerseniz ve JavaScript'teki URL'de "AdminSale" varsa, çiftleşme olabilir.
    // Şimdilik eklemeyelim, action bazında route tanımlayalım.
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

        // Buraya Route attribute'ı ekleyelim.
        // Buradaki adreste /AdminSale kısmı zaten controller adından geldiği için tekrara gerek yok.
        // Eğer JavaScript'teki URL'i birebir eşleştirmek istiyorsak, Route("UpdateOrderStatus") yeterli.
        // Ya da aşağıdaki gibi tam path verebiliriz:
        [HttpPost("/AdminSale/UpdateOrderStatus")] // Bu, tam olarak JavaScript'ten gönderdiğiniz URL'i eşleştirecektir.
        // Ya da sadece [HttpPost("UpdateOrderStatus")] de kullanabilirsiniz, bu durumda
        // geleneksel yönlendirme ile "/AdminSale/UpdateOrderStatus" olarak çalışacaktır.
        // Ancak en güvenlisi tam yolu belirtmektir.
        public async Task<IActionResult> UpdateOrderStatus([FromBody] OrderStatusUpdateModel model) // [FromBody] ve model sınıfı kullanmak daha iyidir.
        {
            // model.orderId ve model.newStatus kullanın
            var order = await _context.Orders.FindAsync(model.OrderId);

            if (order == null)
            {
                return NotFound("Sipariş bulunamadı."); // Hata mesajı döndürebiliriz
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

// JSON body'den gelen verileri karşılamak için bir model sınıfı tanımlayın
public class OrderStatusUpdateModel
{
    public int OrderId { get; set; }
    public string NewStatus { get; set; }
}