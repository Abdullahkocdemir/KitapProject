using Microsoft.AspNetCore.Mvc;
using System.Linq;
using KitapProject.Entities; 
using Microsoft.EntityFrameworkCore;
using KitapProject.Context;

namespace KitapProject.Controllers
{
    public class DefaultController : Controller
    {
        private readonly BookContext _context;

        public DefaultController(BookContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // PopulerProduct ve Status'u true olan ürünleri veritabanından çekiyoruz
            // .Take(6) ile sadece ilk 6 ürünü alıyoruz, bu kısmı ihtiyacınıza göre değiştirebilirsiniz.
            // .Include(p => p.Category) ile Category bilgisini de çekiyoruz, eğer view'da kullanacaksanız önemlidir.
            var popularProducts = _context.Products
                                          .Where(p => p.PopulerProduct && p.Status)
                                          .Include(p => p.Category).Where(y=>y.PopulerProduct==true)
                                          .ToList();

            // Bu listeyi view'a bir model olarak gönderiyoruz
            return View(popularProducts);
        }
    }
}