using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KitapProject.Entities
{
    public class BasketItem
    {
        [Key]
        public int BasketItemId { get; set; }

        [Required]
        [ForeignKey("Cart")]
        public int BasketId { get; set; }
        public virtual Basket Basket { get; set; } = null!;

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "Decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "Decimal(18,2)")]
        public decimal ItemTotalPrice => Quantity * UnitPrice;
    }
}
