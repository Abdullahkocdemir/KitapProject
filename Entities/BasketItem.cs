using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KitapProject.Entities
{
    public class BasketItem
    {
        [Key]
        public int BasketItemId { get; set; }

        // Hangi sepete ait olduğu
        [Required]
        [ForeignKey("Cart")]
        public int BasketId { get; set; }
        public virtual Basket Basket { get; set; } = null!;

        // Hangi ürüne ait olduğu
        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        // Ürünün adedi
        [Required]
        public int Quantity { get; set; }

        // O anki ürünün birim fiyatı (fiyat değişimi durumlarına karşı)
        [Required]
        [Column(TypeName = "Decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        // Toplam kalem tutarı
        [Column(TypeName = "Decimal(18,2)")]
        public decimal ItemTotalPrice => Quantity * UnitPrice;
    }
}
