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

            viewModel.TotalBooks = _context.Products.Count();
            viewModel.TotalCategories = _context.Categories.Count();
            viewModel.TotalTestimonials = _context.Testimonials.Count();
            viewModel.TotalUsers = _context.Users.Count();

            viewModel.LastAddedBooks = _context.Products
                .OrderByDescending(p => p.CreatedDate)
                .Take(5) // Son 5 kitabı al
                .Select(p => new LastAddedBookDTO
                {
                    Name = p.Name,
                    Author = p.Author
                })
                .ToList();
            var ratingCounts = _context.Testimonials
                .GroupBy(t => t.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Rating, x => x.Count);

            for (int i = 1; i <= 5; i++)
            {
                viewModel.TestimonialRatingCounts.Add(ratingCounts.GetValueOrDefault(i, 0));
            }
            var booksByCategory = _context.Products
                .Include(p => p.Category)
                .GroupBy(p => p.Category!.Name)
                .Select(g => new { CategoryName = g.Key, Count = g.Count() })
                .ToList();
            viewModel.BooksByCategoryLabels = booksByCategory.Select(x => x.CategoryName).ToList();
            viewModel.BooksByCategoryCounts = booksByCategory.Select(x => x.Count).ToList();

            return View(viewModel);
        }
    }
}