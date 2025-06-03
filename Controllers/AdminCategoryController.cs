using KitapProject.Context;
using KitapProject.DTO.CategoryDTO;
using KitapProject.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace KitapProject.Controllers
{
    public class AdminCategoryController : Controller
    {
        private readonly BookContext _context;
        private readonly IMapper _mapper;

        public AdminCategoryController(BookContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Project directly to ResultCategoryDTO using AutoMapper for efficiency
                var categoryDtos = await _context.Categories
                                                 .ProjectTo<ResultCategoryDTO>(_mapper.ConfigurationProvider)
                                                 .ToListAsync();

                // Console logging for debugging (optional, remove in production)
                Console.WriteLine($"Mapping sonrası {categoryDtos.Count} DTO oluştu");
                if (categoryDtos.Any())
                {
                    var first = categoryDtos.First();
                    Console.WriteLine($"İlk kategori: ID={first.CategoryId}, Name={first.Name}, Description={first.Description}, Icon={first.Icon}");
                }
                // End Console logging

                return View(categoryDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                // You might want to log the full exception (ex) in a real application
                // Optionally add a user-friendly error message to TempData
                TempData["ErrorMessage"] = "Kategoriler yüklenirken bir hata oluştu.";
                return View(new List<ResultCategoryDTO>()); // Return an empty list on error
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF koruması için önemli
        public async Task<IActionResult> Create(CreateCategoryDTO model)
        {
            if (ModelState.IsValid)
            {
                var category = _mapper.Map<Category>(model);

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Kategori başarıyla eklendi!"; // Add success message for redirect
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost] // AJAX silme işlemi için POST
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                // AJAX isteğine uygun olarak 404 Not Found döndür.
                Response.StatusCode = 404; // Set HTTP status code
                return Content("Kategori bulunamadı."); // Return plain text error message
            }

            // Kategoriye bağlı ürün var mı kontrolü
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                // Eğer bağlı ürünler varsa, silmeye izin verme ve hata mesajı döndür.
                Response.StatusCode = 400; // Set HTTP status code for Bad Request
                return Content("Bu kategoriye bağlı ürünler olduğu için silinemez."); // Return plain text error message
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            // Başarılı olduğunu belirtmek için 200 OK döndür.
            return Ok("Kategori başarıyla silindi.");
        }


        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Güncellenecek kategori bulunamadı.";
                return RedirectToAction(nameof(Index));
            }
            var model = _mapper.Map<UpdateCategoryDTO>(category);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateCategoryDTO model)
        {
            if (ModelState.IsValid)
            {
                var category = await _context.Categories.FindAsync(model.CategoryId);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Güncellenecek kategori bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }
                // Map properties from DTO to entity
                _mapper.Map(model, category);

                // No need to explicitly call Update if entity is tracked and modified
                // _context.Categories.Update(category); 
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Kategori başarıyla güncellendi!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var categoryDto = await _context.Categories
                                             .Where(c => c.CategoryId == id)
                                             .ProjectTo<GetByIdCategoryDTO>(_mapper.ConfigurationProvider)
                                             .FirstOrDefaultAsync();

            if (categoryDto == null)
            {
                TempData["ErrorMessage"] = "Kategori detayı bulunamadı."; // Add message for not found
                return RedirectToAction(nameof(Index)); // Redirect to index if not found
            }
            return View(categoryDto);
        }
    }
}