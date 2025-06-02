using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KitapProject.Entities
{
    public class OrderDetail
    {
        [Key]
        public int OrderDetailId { get; set; }

        // Hangi siparişe ait olduğu
        [Required]
        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

        // Hangi ürüne ait olduğu
        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        // Ürünün adedi
        [Required]
        public int Quantity { get; set; }

        // Sipariş anındaki ürünün birim fiyatı
        [Required]
        [Column(TypeName = "Decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        // Kalemin toplam tutarı
        [Column(TypeName = "Decimal(18,2)")]
        public decimal ItemTotal => Quantity * UnitPrice;
    }
}
