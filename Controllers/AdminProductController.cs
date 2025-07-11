﻿using KitapProject.Context;
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

            return View(productDtos);
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
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CreateProductDTO createProductDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");
                return View(createProductDto);
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == createProductDto.CategoryId);
            if (!categoryExists)
            {
                ModelState.AddModelError("CategoryId", "Seçilen kategori bulunamadı.");
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

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name", product.CategoryId);

            var updateProductDto = _mapper.Map<UpdateProductDTO>(product);
            updateProductDto.CurrentImageUrl = product.ImageUrl;

            return View(updateProductDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] UpdateProductDTO updateProductDto)
        {
            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == updateProductDto.ProductId);

            if (product == null)
            {
                return NotFound($"ID'si {updateProductDto.ProductId} olan ürün bulunamadı.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name", updateProductDto.CategoryId);
                updateProductDto.CurrentImageUrl = product.ImageUrl;
                return View(updateProductDto);
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == updateProductDto.CategoryId);
            if (!categoryExists)
            {
                ModelState.AddModelError("CategoryId", "Seçilen kategori bulunamadı.");
                ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name", updateProductDto.CategoryId);
                updateProductDto.CurrentImageUrl = product.ImageUrl;
                return View(updateProductDto);
            }
            _mapper.Map(updateProductDto, product);

            if (updateProductDto.ImageFile != null && updateProductDto.ImageFile.Length > 0)
            {
                await DeleteImageAsync(product.ImageUrl);
                var newImageUrl = await SaveImageAsync(updateProductDto.ImageFile);
                product.ImageUrl = newImageUrl;
            }

            product.UpdatedDate = DateTime.UtcNow;

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

            return "/Product/" + uniqueFileName;
        }

        private async Task DeleteImageAsync(string? imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl) && imageUrl != "/images/default_product.png")
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    await Task.Run(() => System.IO.File.Delete(filePath));
                }
            }
        }
    }
}