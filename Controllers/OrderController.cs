using Microsoft.AspNetCore.Mvc;
using KitapProject.Context;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using KitapProject.Entities;
using KitapProject.Models; // ViewModel'ler için

namespace KitapProject.Controllers
{
    [Authorize] // Sadece oturum açmış kullanıcılar sipariş oluşturabilir
    public class OrderController : Controller
    {
        private readonly BookContext _context;

        public OrderController(BookContext context)
        {
            _context = context;
        }

        // GET: /Order/Index
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users
                                    .Include(u => u.UserPaymentInfos) // Kayıtlı ödeme bilgilerini yükle
                                    .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                // Kullanıcı bulunamazsa hata sayfasına veya giriş sayfasına yönlendir
                return RedirectToAction("Login", "Account");
            }

            // Sepeti veritabanından çek. Burası artık doğru veriyi içermeli.
            var basket = await _context.Baskets
                .Include(b => b.CartItems)
                .ThenInclude(bi => bi.Product)
                .FirstOrDefaultAsync(b => b.AppUserId == userId);

            // Sepet boşsa kontrolü
            if (basket == null || !basket.CartItems.Any()) // Bu kısım artık null gelmemeli
            {
                TempData["ErrorMessage"] = "Sepetiniz boş. Lütfen önce sepetinize ürün ekleyin.";
                return RedirectToAction("Index", "Basket");
            }

            // Sepet toplamlarını hesapla
            const decimal VAT_RATE = 0.18m; // KDV oranı
            const decimal SHIPPING_FEE = 15.00m; // Kargo ücreti

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
                SavedPaymentInfos = user.UserPaymentInfos.ToList() // Kullanıcının kayıtlı kartlarını ViewModel'e ata
            };

            return View(orderViewModel);
        }

        // POST: /Order/CompleteOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOrder(OrderCreateViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users
                                    .Include(u => u.UserPaymentInfos) // Kart bilgilerini yükle
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

            // Sepet toplamlarını tekrar hesapla (client tarafında manipülasyon olmaması için sunucuda da hesaplanır)
            const decimal VAT_RATE = 0.18m;
            const decimal SHIPPING_FEE = 15.00m;
            decimal booksSubtotal = basket.CartItems.Sum(bi => bi.ItemTotalPrice);
            decimal vatAmount = booksSubtotal * VAT_RATE;
            decimal calculatedTotalAmount = booksSubtotal + vatAmount + SHIPPING_FEE;

            // ViewModel'deki TotalAmount'ı güncelleyelim (varsa, client'tan gelen değer değil, hesaplanan değer olsun)
            model.TotalAmount = calculatedTotalAmount;
            model.VATAmount = vatAmount;
            model.ShippingFee = SHIPPING_FEE;
            model.BasketItems = basket.CartItems.ToList(); // Sepet öğelerini tekrar ViewModel'e ata (doğrulama hatası olursa görüntülemek için)
            model.SavedPaymentInfos = user.UserPaymentInfos.ToList(); // Kayıtlı kartları tekrar ViewModel'e ata

            // Adres bilgilerini doğrulama
            if (string.IsNullOrWhiteSpace(model.ShippingAddress) ||
                string.IsNullOrWhiteSpace(model.City) ||
                string.IsNullOrWhiteSpace(model.Country) ||
                string.IsNullOrWhiteSpace(model.PostalCode))
            {
                ModelState.AddModelError("", "Lütfen tüm teslimat bilgilerini eksiksiz doldurun.");
            }

            // Ödeme bilgilerini doğrulama
            if (model.SelectedPaymentInfoId.HasValue) // Kayıtlı kart seçilmişse
            {
                var selectedCard = user.UserPaymentInfos.FirstOrDefault(pi => pi.PaymentInfoId == model.SelectedPaymentInfoId.Value);
                if (selectedCard == null)
                {
                    ModelState.AddModelError("", "Geçersiz kayıtlı kart seçimi.");
                }
                // Seçilen kart bilgileri zaten veritabanından alınacak, ekstra doğrulama şimdilik gerekmeyebilir.
            }
            else // Yeni kart bilgileri girilmişse
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
                    // Basit kart formatı doğrulamaları
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
                return View("Index", model); // Model geçerli değilse aynı sayfaya geri dön
            }

            // Adres bilgilerini güncelle
            user.ShippingAddress = model.ShippingAddress;
            user.City = model.City;
            user.Country = model.Country;
            user.PostalCode = model.PostalCode;
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);

            // Ödeme Bilgilerini Kaydet/Kullan
            UserPaymentInfo? paymentInfoToUse = null;
            if (model.SelectedPaymentInfoId.HasValue)
            {
                // Seçilen kayıtlı kartı kullan
                paymentInfoToUse = user.UserPaymentInfos.FirstOrDefault(pi => pi.PaymentInfoId == model.SelectedPaymentInfoId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(model.CardNumber)) // Yeni kart bilgileri girilmişse
            {
                // Yeni kart bilgilerini kaydet (isteğe bağlı)
                // Gerçek uygulamada buraya ödeme sağlayıcısı ile tokenization entegrasyonu gelir.
                // Bu örnekte sadece kartın son 4 hanesini kaydediyoruz.
                paymentInfoToUse = new UserPaymentInfo
                {
                    AppUserId = userId!,
                    CardHolderName = model.CardHolderName!,
                    CardNumberLastFour = model.CardNumber!.Replace(" ", "").Substring(model.CardNumber.Replace(" ", "").Length - 4),
                    ExpirationDate = model.ExpirationDate!,
                    CVV = model.CVV!, // Geliştirme ortamında saklanır, canlıda saklanmaz
                    IsDefault = false // Yeni kart varsayılan olmasın, daha sonra kullanıcı ayarlayabilir
                };
                _context.UserPaymentInfos.Add(paymentInfoToUse);
            }

            // Ödeme İşlemi (Simülasyon)
            // Gerçek bir ödeme ağ geçidi entegrasyonu burada yapılır.
            // Örneğin: var paymentResult = await _paymentService.ProcessPayment(model.CardNumber, model.ExpirationDate, model.CVV, calculatedTotalAmount);
            // if (!paymentResult.IsSuccess) {
            //    ModelState.AddModelError("", "Ödeme işlemi başarısız oldu: " + paymentResult.ErrorMessage);
            //    return View("Index", model);
            // }

            // Sipariş Oluşturma
            var order = new Order
            {
                AppUserId = userId!,
                OrderDate = DateTime.UtcNow,
                TotalAmount = calculatedTotalAmount, // Hesaplanan toplam tutarı kullan
                OrderStatus = "Beklemede",
                ShippingAddress = model.ShippingAddress,
                City = model.City,
                Country = model.Country,
                PostalCode = model.PostalCode
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // OrderId'nin oluşması için kaydet

            // Sipariş Detaylarını Oluşturma
            foreach (var item in basket.CartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice // Sepet anındaki birim fiyatı kullan
                };
                _context.OrderDetails.Add(orderDetail);
            }

            // Sepeti Temizle
            _context.BasketItems.RemoveRange(basket.CartItems);
            _context.Baskets.Remove(basket);

            // Fatura Oluşturma
            var invoice = new Invoice
            {
                OrderId = order.OrderId,
                InvoiceDate = DateTime.UtcNow,
                Amount = order.TotalAmount,
                InvoiceStatus = "Oluşturuldu",
                InvoiceNumber = Guid.NewGuid().ToString() // Otomatik bir fatura numarası oluştur
            };
            _context.Invoices.Add(invoice);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Siparişiniz başarıyla alındı ve onaylandı!";
            return RedirectToAction("Index", "Default"); // Başarılıysa anasayfaya yönlendir
        }
    }
}