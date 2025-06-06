using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KitapProject.Entities
{
    public class OrderDetail
    {
        [Key]
        public int OrderDetailId { get; set; }

        [Required]
        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

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
        public decimal ItemTotal => Quantity * UnitPrice;
    }
}
