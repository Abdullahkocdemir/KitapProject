// KitapProject.Models/OrderCreateViewModel.cs
using KitapProject.Entities;
using System.ComponentModel.DataAnnotations;

namespace KitapProject.Models
{
    public class OrderCreateViewModel
    {
        // Teslimat Bilgileri
        [Required(ErrorMessage = "Gönderim adresi gereklidir.")]
        [StringLength(250, ErrorMessage = "Gönderim adresi en fazla 250 karakter olabilir.")]
        [Display(Name = "Gönderim Adresi")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şehir bilgisi gereklidir.")]
        [StringLength(50, ErrorMessage = "Şehir en fazla 50 karakter olabilir.")]
        [Display(Name = "Şehir")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ülke bilgisi gereklidir.")]
        [StringLength(50, ErrorMessage = "Ülke en fazla 50 karakter olabilir.")]
        [Display(Name = "Ülke")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "Posta kodu gereklidir.")]
        [StringLength(10, ErrorMessage = "Posta kodu en fazla 10 karakter olabilir.")]
        [Display(Name = "Posta Kodu")]
        public string PostalCode { get; set; } = string.Empty;

        // Ödeme Bilgileri
        // Yeni kart eklenecekse bu alanlar kullanılacak
        [Display(Name = "Kart Sahibi Adı")]
        [StringLength(100, ErrorMessage = "Kart sahibi adı en fazla 100 karakter olabilir.")]
        public string? CardHolderName { get; set; }

        [Display(Name = "Kart Numarası")]
        [StringLength(19, MinimumLength = 16, ErrorMessage = "Kart numarası 16 ila 19 karakter olmalıdır.")]
        public string? CardNumber { get; set; } // Tam kart numarası sadece işlem anında kullanılacak

        [Display(Name = "Son Kullanma Tarihi")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Son kullanma tarihi AA/YY formatında olmalıdır.")]
        public string? ExpirationDate { get; set; }

        [Display(Name = "CVV")]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV 3 veya 4 haneli olmalıdır.")]
        public string? CVV { get; set; }

        // Mevcut kart seçimi için
        [Display(Name = "Kayıtlı Kartlar")]
        public int? SelectedPaymentInfoId { get; set; } // Seçilen kayıtlı kartın ID'si

        public List<UserPaymentInfo>? SavedPaymentInfos { get; set; } // Kullanıcının kayıtlı kartları

        // Sepet ve Toplam Bilgileri
        public List<BasketItem> BasketItems { get; set; } = new List<BasketItem>();
        public decimal TotalAmount { get; set; }
        public decimal VATAmount { get; set; } // KDV tutarı
        public decimal ShippingFee { get; set; } // Kargo ücreti
    }
}