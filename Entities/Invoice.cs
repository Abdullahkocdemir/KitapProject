using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KitapProject.Entities
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        // Hangi sipariş için fatura kesildiği
        [Required]
        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

        // Fatura numarası (benzersiz olmalı)
        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; } = Guid.NewGuid().ToString(); // Otomatik oluşturulabilir

        // Fatura tarihi
        public DateTime InvoiceDate { get; set; } = DateTime.Now;

        // Fatura tutarı (siparişin toplam tutarı ile aynı olmalı)
        [Required]
        [Column(TypeName = "Decimal(18,2)")]
        public decimal Amount { get; set; }

        // Fatura durumu (Ödendi, İptal Edildi, Beklemede vb.)
        [Required]
        [StringLength(50)]
        public string InvoiceStatus { get; set; } = "Oluşturuldu";

        // Fatura PDF veya URL'si (eğer varsa)
        public string? InvoiceUrl { get; set; }
    }
}
