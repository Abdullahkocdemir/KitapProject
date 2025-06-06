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
                var categoryDtos = await _context.Categories
                                                 .ProjectTo<ResultCategoryDTO>(_mapper.ConfigurationProvider)
                                                 .ToListAsync();

                Console.WriteLine($"Mapping sonrası {categoryDtos.Count} DTO oluştu");
                if (categoryDtos.Any())
                {
                    var first = categoryDtos.First();
                    Console.WriteLine($"İlk kategori: ID={first.CategoryId}, Name={first.Name}, Description={first.Description}, Icon={first.Icon}");
                }

                return View(categoryDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                TempData["ErrorMessage"] = "Kategoriler yüklenirken bir hata oluştu.";
                return View(new List<ResultCategoryDTO>()); 
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Create(CreateCategoryDTO model)
        {
            if (ModelState.IsValid)
            {
                var category = _mapper.Map<Category>(model);

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Kategori başarıyla eklendi!"; 
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost] 
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                Response.StatusCode = 404; 
                return Content("Kategori bulunamadı."); 
            }

            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                Response.StatusCode = 400; 
                return Content("Bu kategoriye bağlı ürünler olduğu için silinemez."); 
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

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
                _mapper.Map(model, category);
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
                TempData["ErrorMessage"] = "Kategori detayı bulunamadı."; 
                return RedirectToAction(nameof(Index)); 
            }
            return View(categoryDto);
        }
    }
}