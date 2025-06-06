using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KitapProject.Entities
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required]
        [Column(TypeName = "VarChar")]
        [StringLength(30)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "VarChar")]
        [StringLength(50)]
        public string Author { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "VarChar")]
        [StringLength(600)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "VarChar")]
        [StringLength(200)]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        public bool Status { get; set; } = true;

        public bool PopulerProduct { get; set; }

        [Required]
        [Column(TypeName = "Decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public virtual Category? Category { get; set; }
    }
}