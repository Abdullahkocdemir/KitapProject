using KitapProject.Context;
using KitapProject.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitapProject.Controllers
{


    public class AddToCartDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1; 
    }

    public class ProductController : Controller
    {
        private readonly BookContext _context;

        public ProductController(BookContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(string? categoryName, string? searchTerm)
        {
            IQueryable<Product> query = _context.Products.Include(p => p.Category);

            if (!string.IsNullOrEmpty(categoryName) && categoryName.ToLower() != "all")
            {
                query = query.Where(p => p.Category != null && p.Category.Name.ToLower() == categoryName.ToLower());
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Author.ToLower().Contains(searchTerm) ||
                    (p.Category != null && p.Category.Name.ToLower().Contains(searchTerm))
                );
            }

            var products = await query
                .Select(p => new
                {
                    id = p.ProductId,
                    title = p.Name,
                    author = p.Author,
                    price = p.Price,
                    category = p.Category != null ? p.Category.Name.ToLower() : "unknown", 
                    image = p.ImageUrl 
                })
                .ToListAsync();

            if (!products.Any())
            {
                return Ok(new List<object>());
            }

            return Ok(products);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .Select(c => new
                {
                    name = c.Name.ToLower(), 
                    displayName = c.Name,
                    icon = c.Icon
                })
                .ToListAsync();

            var allCategory = new { name = "all", displayName = "Tümü", icon = "fas fa-book-reader" }; 
            var categoryList = new List<object> { allCategory };
            categoryList.AddRange(categories);

            return Ok(categoryList);
        }

    }
}