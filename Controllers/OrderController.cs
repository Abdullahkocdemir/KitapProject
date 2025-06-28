using Microsoft.AspNetCore.Mvc;
using KitapProject.Context;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using KitapProject.Entities;
using KitapProject.Models;

namespace KitapProject.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly BookContext _context;

        public OrderController(BookContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users
                                    .Include(u => u.UserPaymentInfos)
                                    .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {

                return RedirectToAction("Login", "Account");
            }

            var basket = await _context.Baskets
                .Include(b => b.CartItems)
                .ThenInclude(bi => bi.Product)
                .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (basket == null || !basket.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Sepetiniz boş. Lütfen önce sepetinize ürün ekleyin.";
                return RedirectToAction("Index", "Basket");
            }

            const decimal VAT_RATE = 0.18m;
            const decimal SHIPPING_FEE = 15.00m;

            decimal booksSubtotal = basket.CartItems.Sum(bi => bi.ItemTotalPrice);
            decimal vatAmount = booksSubtotal * VAT_RATE;
            decimal finalTotal = booksSubtotal + vatAmount + SHIPPING_FEE;


            var orderViewModel = new OrderCreateViewModel
            {
                ShippingAddress = user.ShippingAddress ?? "",
                City = user.City ?? "",
                Country = user.Country ?? "",
                PostalCode = user.PostalCode ?? "",
                BasketItems = basket.CartItems.ToList(),
                TotalAmount = finalTotal,
                VATAmount = vatAmount,
                ShippingFee = SHIPPING_FEE,
                SavedPaymentInfos = user.UserPaymentInfos.ToList()
            };

            return View(orderViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOrder(OrderCreateViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users
                                    .Include(u => u.UserPaymentInfos)
                                    .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var basket = await _context.Baskets
                .Include(b => b.CartItems)
                .ThenInclude(bi => bi.Product)
                .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (basket == null || !basket.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Siparişinizi tamamlamak için sepetinizde ürün bulunmamaktadır.";
                return RedirectToAction("Index", "Basket");
            }

            const decimal VAT_RATE = 0.18m;
            const decimal SHIPPING_FEE = 15.00m;
            decimal booksSubtotal = basket.CartItems.Sum(bi => bi.ItemTotalPrice);
            decimal vatAmount = booksSubtotal * VAT_RATE;
            decimal calculatedTotalAmount = booksSubtotal + vatAmount + SHIPPING_FEE;

            model.TotalAmount = calculatedTotalAmount;
            model.VATAmount = vatAmount;
            model.ShippingFee = SHIPPING_FEE;
            model.BasketItems = basket.CartItems.ToList();
            model.SavedPaymentInfos = user.UserPaymentInfos.ToList();

            if (string.IsNullOrWhiteSpace(model.ShippingAddress) ||
                string.IsNullOrWhiteSpace(model.City) ||
                string.IsNullOrWhiteSpace(model.Country) ||
                string.IsNullOrWhiteSpace(model.PostalCode))
            {
                ModelState.AddModelError("", "Lütfen tüm teslimat bilgilerini eksiksiz doldurun.");
            }

            if (model.SelectedPaymentInfoId.HasValue)
            {
                var selectedCard = user.UserPaymentInfos.FirstOrDefault(pi => pi.PaymentInfoId == model.SelectedPaymentInfoId.Value);
                if (selectedCard == null)
                {
                    ModelState.AddModelError("", "Geçersiz kayıtlı kart seçimi.");
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(model.CardHolderName) ||
                    string.IsNullOrWhiteSpace(model.CardNumber) ||
                    string.IsNullOrWhiteSpace(model.ExpirationDate) ||
                    string.IsNullOrWhiteSpace(model.CVV))
                {
                    ModelState.AddModelError("", "Lütfen yeni kart bilgilerinizi eksiksiz doldurun.");
                }
                else
                {
                    if (model.CardNumber.Replace(" ", "").Length < 16 || model.CardNumber.Replace(" ", "").Length > 19)
                    {
                        ModelState.AddModelError("CardNumber", "Geçerli bir kart numarası girin.");
                    }
                    if (!System.Text.RegularExpressions.Regex.IsMatch(model.ExpirationDate, @"^(0[1-9]|1[0-2])\/\d{2}$"))
                    {
                        ModelState.AddModelError("ExpirationDate", "Son kullanma tarihi AA/YY formatında olmalıdır.");
                    }
                    if (model.CVV.Length < 3 || model.CVV.Length > 4)
                    {
                        ModelState.AddModelError("CVV", "CVV 3 veya 4 haneli olmalıdır.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            user.ShippingAddress = model.ShippingAddress;
            user.City = model.City;
            user.Country = model.Country;
            user.PostalCode = model.PostalCode;
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);

            UserPaymentInfo? paymentInfoToUse = null;
            if (model.SelectedPaymentInfoId.HasValue)
            {
                paymentInfoToUse = user.UserPaymentInfos.FirstOrDefault(pi => pi.PaymentInfoId == model.SelectedPaymentInfoId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(model.CardNumber))
            {
                paymentInfoToUse = new UserPaymentInfo
                {
                    AppUserId = userId!,
                    CardHolderName = model.CardHolderName!,
                    CardNumberLastFour = model.CardNumber!.Replace(" ", "").Substring(model.CardNumber.Replace(" ", "").Length - 4),
                    ExpirationDate = model.ExpirationDate!,
                    CVV = model.CVV!,
                    IsDefault = false
                };
                _context.UserPaymentInfos.Add(paymentInfoToUse);
            }

            var order = new Order
            {
                AppUserId = userId!,
                OrderDate = DateTime.UtcNow,
                TotalAmount = calculatedTotalAmount,
                OrderStatus = "Beklemede",
                ShippingAddress = model.ShippingAddress,
                City = model.City,
                Country = model.Country,
                PostalCode = model.PostalCode
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in basket.CartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                };
                _context.OrderDetails.Add(orderDetail);
            }

            _context.BasketItems.RemoveRange(basket.CartItems);
            _context.Baskets.Remove(basket);

            var invoice = new Invoice
            {
                OrderId = order.OrderId,
                InvoiceDate = DateTime.UtcNow,
                Amount = order.TotalAmount,
                InvoiceStatus = "Oluşturuldu",
                InvoiceNumber = Guid.NewGuid().ToString()
            };
            _context.Invoices.Add(invoice);

            await _context.SaveChangesAsync();

            // AJAX isteği için JSON response döndür
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                Request.Headers["Content-Type"].ToString().Contains("application/json"))
            {
                return Json(new
                {
                    success = true,
                    message = "Siparişiniz başarıyla alındı ve onaylandı!",
                    orderNumber = order.OrderId,
                    totalAmount = calculatedTotalAmount.ToString("N2")
                });
            }

            // Normal form submit için TempData kullan
            TempData["OrderSuccess"] = "true";
            TempData["OrderNumber"] = order.OrderId.ToString();
            TempData["TotalAmount"] = calculatedTotalAmount.ToString("N2");
            return RedirectToAction("Index", "Default");
        }
    }
}