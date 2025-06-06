using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using KitapProject.Entities;
using KitapProject.Context;

namespace KitapProject.Controllers
{
    public class BasketController : Controller
    {
        private readonly BookContext _context;

        public BasketController(BookContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var basket = await _context.Baskets
                .Include(b => b.CartItems)
                .ThenInclude(bi => bi.Product)
                .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (basket == null)
            {
                basket = new Basket { AppUserId = userId!, CreatedDate = DateTime.UtcNow, TotalPrice = 0 };
                _context.Baskets.Add(basket);
                await _context.SaveChangesAsync();
            }

            return View(basket);
        }

        [HttpPost]
        [Authorize]
        public IActionResult ConfirmBasket()
        {
            return RedirectToAction("Index", "Order");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToBasket(int productId, int quantity = 1)
        {
            Console.WriteLine($"AddToBasket çağrıldı: ProductId={productId}, Quantity={quantity}");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = await _context.Products.FindAsync(productId);

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Sepete ürün eklemek için giriş yapmalısınız." });
            }
            if (product == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı." });
            }

            var basket = await _context.Baskets
                .Include(b => b.CartItems)
                .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (basket == null)
            {
                basket = new Basket { AppUserId = userId, CreatedDate = DateTime.UtcNow, TotalPrice = 0 };
                _context.Baskets.Add(basket);
                await _context.SaveChangesAsync();
            }

            var basketItem = basket.CartItems.FirstOrDefault(bi => bi.ProductId == productId);

            if (basketItem != null)
            {
                basketItem.Quantity += quantity;
                basketItem.UnitPrice = product.Price;
            }
            else
            {
                basketItem = new BasketItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    BasketId = basket.BasketId
                };
                basket.CartItems.Add(basketItem);
            }

            basket.TotalPrice = basket.CartItems.Sum(bi => bi.ItemTotalPrice);
            basket.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var currentBasketItemCount = basket.CartItems.Sum(bi => bi.Quantity);
            return Json(new { success = true, message = "Ürün sepete eklendi.", newTotal = basket.TotalPrice, itemCount = currentBasketItemCount });
        }

        [HttpGet]
        public IActionResult CheckLoginStatus()
        {
            bool isLoggedIn = User.Identity?.IsAuthenticated ?? false;
            return Json(new { isLoggedIn = isLoggedIn });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateBasketItemQuantity([FromBody] UpdateQuantityModel model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var basket = await _context.Baskets
                    .Include(b => b.CartItems)
                    .FirstOrDefaultAsync(b => b.AppUserId == userId);

                if (basket == null)
                {
                    return Json(new { success = false, message = "Sepet bulunamadı." });
                }

                var basketItem = basket.CartItems.FirstOrDefault(bi => bi.ProductId == model.ProductId);

                if (basketItem == null)
                {
                    return Json(new { success = false, message = "Ürün sepette bulunamadı." });
                }

                basketItem.Quantity += model.Change;

                if (basketItem.Quantity <= 0)
                {
                    basket.CartItems.Remove(basketItem);
                    _context.BasketItems.Remove(basketItem);
                }

                basket.TotalPrice = basket.CartItems.Where(bi => bi.Quantity > 0).Sum(bi => bi.ItemTotalPrice);
                basket.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    newQuantity = basketItem.Quantity <= 0 ? 0 : basketItem.Quantity,
                    newTotalPrice = basket.TotalPrice,
                    itemRemoved = basketItem.Quantity <= 0
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateBasketItemQuantity error: {ex.Message}");
                return Json(new { success = false, message = "Sepet güncellenirken bir hata oluştu." });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveFromBasket([FromBody] RemoveItemModel model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var basket = await _context.Baskets
                    .Include(b => b.CartItems)
                    .FirstOrDefaultAsync(b => b.AppUserId == userId);

                if (basket == null)
                {
                    return Json(new { success = false, message = "Sepet bulunamadı." });
                }

                var basketItem = basket.CartItems.FirstOrDefault(bi => bi.ProductId == model.ProductId);

                if (basketItem == null)
                {
                    return Json(new { success = false, message = "Ürün sepette bulunamadı." });
                }

                basket.CartItems.Remove(basketItem);
                _context.BasketItems.Remove(basketItem);

                // Toplam fiyatı yeniden hesapla
                basket.TotalPrice = basket.CartItems.Sum(bi => bi.ItemTotalPrice);
                basket.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Ürün sepetten silindi.", newTotalPrice = basket.TotalPrice });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RemoveFromBasket error: {ex.Message}");
                return Json(new { success = false, message = "Ürün silinirken bir hata oluştu." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCartItemCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { count = 0 });
            }

            var basket = await _context.Baskets
                                       .Include(b => b.CartItems)
                                       .FirstOrDefaultAsync(b => b.AppUserId == userId);

            var itemCount = basket?.CartItems.Sum(bi => bi.Quantity) ?? 0;
            return Json(new { count = itemCount });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetBasketItems()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Giriş yapmadınız.", items = new List<object>(), total = 0.0m });
            }

            var basket = await _context.Baskets
                .Include(b => b.CartItems)
                .ThenInclude(bi => bi.Product)
                .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (basket == null)
            {
                return Json(new { success = true, items = new List<object>(), total = 0.0m });
            }

            var items = basket.CartItems.Select(bi => new
            {
                id = bi.ProductId,
                name = bi.Product.Name,
                price = bi.UnitPrice,
                quantity = bi.Quantity,
                imageUrl = bi.Product.ImageUrl
            }).ToList();

            return Json(new { success = true, items = items, total = basket.TotalPrice });
        }

        public class UpdateQuantityModel
        {
            public int ProductId { get; set; }
            public int Change { get; set; }
        }

        public class RemoveItemModel
        {
            public int ProductId { get; set; }
        }
    }
}