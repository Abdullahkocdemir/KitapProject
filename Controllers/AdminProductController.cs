using KitapProject.Context;
using Microsoft.AspNetCore.Mvc;
using KitapProject.DTO.ProductDTO;
using KitapProject.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KitapProject.Controllers
{
    public class AdminProductController : Controller
    {
        private readonly BookContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminProductController(BookContext context, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");

            var productDtos = await _context.Products
                                            .Include(p => p.Category)
                                            .ProjectTo<ResultProductDTO>(_mapper.ConfigurationProvider)
                                            .ToListAsync();

            return View(productDtos); // Sending List<ResultProductDTO>
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var productDto = await _context.Products
                                               .Where(p => p.ProductId == id)
                                               .ProjectTo<GetByIdProductDTO>(_mapper.ConfigurationProvider)
                                               .FirstOrDefaultAsync();

            if (productDto == null)
            {
                return NotFound();
            }

            return View(productDto);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Kategorileri veritabanından çek ve ViewBag'e List<SelectListItem> olarak ata
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CreateProductDTO createProductDto)
        {
            if (!ModelState.IsValid)
            {
                // ModelState geçerli değilse, kategorileri tekrar yükle
                ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");
                return View(createProductDto);
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == createProductDto.CategoryId);
            if (!categoryExists)
            {
                ModelState.AddModelError("CategoryId", "Seçilen kategori bulunamadı.");
                // Kategori bulunamazsa, kategorileri tekrar yükle
                ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");
                return View(createProductDto);
            }

            var product = _mapper.Map<Product>(createProductDto);

            if (createProductDto.ImageFile != null && createProductDto.ImageFile.Length > 0)
            {
                var imageUrl = await SaveImageAsync(createProductDto.ImageFile);
                product.ImageUrl = imageUrl;
            }
            else
            {
                product.ImageUrl = "/images/default_product.png";
            }

            product.CreatedDate = DateTime.UtcNow;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Kategorileri veritabanından çek ve ViewBag'e List<SelectListItem> olarak ata
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name", product.CategoryId);

            var updateProductDto = _mapper.Map<UpdateProductDTO>(product);
            updateProductDto.CurrentImageUrl = product.ImageUrl;

            return View(updateProductDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] UpdateProductDTO updateProductDto)
        {
            if (!ModelState.IsValid)
            {
                // ModelState geçerli değilse, kategorileri tekrar yükle ve mevcut resim URL'sini tut
                // AsNoTracking kullanmak, mevcut Product nesnesinin takip edilmesini engeller, bu sayede View'e gönderilen DTO'nun performansı artar.
                var productForView = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == updateProductDto.ProductId);
                ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name", updateProductDto.CategoryId);
                if (productForView != null)
                {
                    updateProductDto.CurrentImageUrl = productForView.ImageUrl;
                }
                return View(updateProductDto);
            }

            var product = await _context.Products.FindAsync(updateProductDto.ProductId);
            if (product == null)
            {
                return NotFound($"ID'si {updateProductDto.ProductId} olan ürün bulunamadı.");
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == updateProductDto.CategoryId);
            if (!categoryExists)
            {
                ModelState.AddModelError("CategoryId", "Seçilen kategori bulunamadı.");
                // Kategori bulunamazsa, kategorileri tekrar yükle ve mevcut resim URL'sini tut
                ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name", updateProductDto.CategoryId);
                updateProductDto.CurrentImageUrl = product.ImageUrl;
                return View(updateProductDto);
            }

            // Yeni resim yüklendiyse
            if (updateProductDto.ImageFile != null && updateProductDto.ImageFile.Length > 0)
            {
                // Eski resmi sil (varsayılan resim değilse)
                await DeleteImageAsync(product.ImageUrl);

                // Yeni resmi kaydet
                var newImageUrl = await SaveImageAsync(updateProductDto.ImageFile);
                product.ImageUrl = newImageUrl;
            }

            // Diğer alanları güncelle
            product.Name = updateProductDto.Name;
            product.Author = updateProductDto.Author;
            product.Description = updateProductDto.Description;
            product.Price = updateProductDto.Price;
            product.Status = updateProductDto.Status;
            product.PopulerProduct = updateProductDto.PopulerProduct;
            product.CategoryId = updateProductDto.CategoryId;
            product.UpdatedDate = DateTime.UtcNow; // Evrensel saat kullanılıyor

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            await DeleteImageAsync(product.ImageUrl);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            // Dosyayı wwwroot/Product klasörüne kaydet
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Product");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            // Veritabanına kaydedilecek göreceli yolu döndür
            return "/Product/" + uniqueFileName;
        }

        private async Task DeleteImageAsync(string? imageUrl)
        {
            // Varsayılan resim değilse ve URL boş değilse resmi sil
            if (!string.IsNullOrEmpty(imageUrl) && imageUrl != "/images/default_product.png")
            {
                // `_webHostEnvironment.WebRootPath` kullanarak fiziksel yolu oluştur
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    // Dosyayı asenkron olarak sil
                    await Task.Run(() => System.IO.File.Delete(filePath));
                }
            }
        }
    }
}