using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KitapProject.Entities
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        [Required]
        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; } = Guid.NewGuid().ToString();

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "Decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string InvoiceStatus { get; set; } = "Oluşturuldu";

        public string? InvoiceUrl { get; set; }
    }
}