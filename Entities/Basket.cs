using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KitapProject.Entities
{
    public class Basket
    {
        [Key] 
        public int BasketId { get; set; }

        [Required]
        [ForeignKey("AppUser")]
        public string AppUserId { get; set; } = string.Empty;
        public virtual AppUser AppUser { get; set; } = null!;

        // DateTime.Now yerine DateTime.UtcNow
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        [Column(TypeName = "Decimal(18,2)")]
        public decimal TotalPrice { get; set; } = 0;

        public virtual ICollection<BasketItem> CartItems { get; set; } = new List<BasketItem>();
    }
}