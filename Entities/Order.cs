using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KitapProject.Entities
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        // Siparişi veren kullanıcı
        [Required]
        [ForeignKey("AppUser")]
        public string AppUserId { get; set; } = string.Empty;
        public virtual AppUser AppUser { get; set; } = null!;

        // Siparişin verildiği tarih
        public DateTime OrderDate { get; set; } = DateTime.Now;

        // Siparişin toplam tutarı
        [Required]
        [Column(TypeName = "Decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // Sipariş durumu (Beklemede, Hazırlanıyor, Kargolandı, Teslim Edildi, İptal Edildi vb.)
        [Required]
        [StringLength(50)]
        public string OrderStatus { get; set; } = "Beklemede"; // Varsayılan durum

        // Kargo Adresi Bilgileri
        [Required]
        [StringLength(250)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Country { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string PostalCode { get; set; } = string.Empty;

        // Sipariş detayları
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    }
}
