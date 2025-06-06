using KitapProject.Context;
using Microsoft.AspNetCore.Mvc;
using System.Linq; 
using Microsoft.EntityFrameworkCore;
using KitapProject.DTO.DashBoardDTO; 

namespace KitapProject.Controllers
{
    public class AdminDashBoardController : Controller
    {
        private readonly BookContext _context;

        public AdminDashBoardController(BookContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var viewModel = new ResultDashBoardDTO();

            // 1. Toplam Sayılar
            viewModel.TotalBooks = _context.Products.Count();
            viewModel.TotalCategories = _context.Categories.Count();
            viewModel.TotalTestimonials = _context.Testimonials.Count();
            viewModel.TotalUsers = _context.Users.Count(); // IdentityUsers'lar için AppUser'ı kullanır

            // 2. Son Eklenen Kitaplar (Örnek: Son 5 kitap)
            viewModel.LastAddedBooks = _context.Products
                .OrderByDescending(p => p.CreatedDate)
                .Take(5) // Son 5 kitabı al
                .Select(p => new LastAddedBookDTO // Sadece gerekli alanları seç
                {
                    Name = p.Name,
                    Author = p.Author
                })
                .ToList();

            // 3. Referans Derecelendirme Dağılımı
            // Rating 1'den 5'e kadar olduğu için, her bir rating değerinin kaç kez geçtiğini sayarız.
            // Boş derecelendirmeler için 0 değeri dönecek şekilde ayarlandı.
            var ratingCounts = _context.Testimonials
                .GroupBy(t => t.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Rating, x => x.Count);

            for (int i = 1; i <= 5; i++)
            {
                viewModel.TestimonialRatingCounts.Add(ratingCounts.GetValueOrDefault(i, 0));
            }

            // 4. Kategoriye Göre Kitap Sayısı
            var booksByCategory = _context.Products
                .Include(p => p.Category) // Category navigasyon özelliğini yükle
                .GroupBy(p => p.Category!.Name) // Kategori adına göre grupla
                .Select(g => new { CategoryName = g.Key, Count = g.Count() })
                .ToList();

            viewModel.BooksByCategoryLabels = booksByCategory.Select(x => x.CategoryName).ToList();
            viewModel.BooksByCategoryCounts = booksByCategory.Select(x => x.Count).ToList();

            return View(viewModel); // ViewModel'i View'a gönder
        }
    }
}