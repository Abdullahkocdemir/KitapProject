using KitapProject.Context;
using Microsoft.AspNetCore.Mvc;
using KitapProject.DTO.ProductDTO;
using KitapProject.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace KitapProject.Controllers
{

    public class ProductController : Controller
    {
        private readonly BookContext _context;
        private readonly IMapper _mapper;

        public ProductController(BookContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index() 
        {
            var productDtos = await _context.Products
                                            .ProjectTo<ResultProductDTO>(_mapper.ConfigurationProvider)
                                            .ToListAsync();

            return Ok(productDtos);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound($"ID'si {id} olan ürün bulunamadı.");
            }

            var productDto = new GetByIdProductDTO
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Author = product.Author,
                Description = product.Description,
                ImageURl = product.ImageURl,
                CreatedDate = product.CreatedDate,
                UpdatedDate = product.UpdatedDate,
                Status = product.Status,
                PopulerProduct = product.PopulerProduct,
                Price = product.Price,
                CategoryId = product.CategoryId
            };
            return Ok(productDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDTO createProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == createProductDto.CategoryId);
            if (!categoryExists)
            {
                return BadRequest($"CategoryId {createProductDto.CategoryId} bulunamadı.");
            }

            var product = new Product
            {
                Name = createProductDto.Name,
                Author = createProductDto.Author,
                Description = createProductDto.Description,
                ImageURl = createProductDto.ImageURl,
                Price = createProductDto.Price,
                CategoryId = createProductDto.CategoryId,
                CreatedDate = DateTime.Now, // Oluşturulma tarihi otomatik belirlenir
                Status = true, // Varsayılan olarak aktif
                PopulerProduct = false // Varsayılan olarak popüler değil
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductById), new { id = product.ProductId }, product);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductDTO updateProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _context.Products.FindAsync(updateProductDto.ProductId);
            if (product == null)
            {
                return NotFound($"ID'si {updateProductDto.ProductId} olan ürün bulunamadı.");
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == updateProductDto.CategoryId);
            if (!categoryExists)
            {
                return BadRequest($"CategoryId {updateProductDto.CategoryId} bulunamadı.");
            }

            product.Name = updateProductDto.Name;
            product.Author = updateProductDto.Author;
            product.Description = updateProductDto.Description;
            product.ImageURl = updateProductDto.ImageURl;
            product.Price = updateProductDto.Price;
            product.Status = updateProductDto.Status;
            product.PopulerProduct = updateProductDto.PopulerProduct;
            product.CategoryId = updateProductDto.CategoryId;
            product.UpdatedDate = DateTime.Now; // Güncelleme tarihi otomatik belirlenir

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return NoContent(); // Başarılı güncelleme için 204 No Content
        }

        /// <summary>
        /// Belirtilen ID'ye sahip ürünü siler.
        /// </summary>
        /// <param name="id">Silinecek ürün ID'si</param>
        /// <returns>Silme sonucu</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound($"ID'si {id} olan ürün bulunamadı.");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent(); // Başarılı silme için 204 No Content
        }
    }
}