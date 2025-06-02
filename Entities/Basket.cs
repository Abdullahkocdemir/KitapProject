using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KitapProject.Entities
{
    public class Basket
    {
        [Key]
        public int BasketId { get; set; }

        // Sepetin sahibi olan kullanıcı
        [Required]
        [ForeignKey("AppUser")]
        public string AppUserId { get; set; } = string.Empty; // IdentityUser'dan geldiği için string
        public virtual AppUser AppUser { get; set; } = null!;

        // Sepetin oluşturulma tarihi
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Sepetin son güncellenme tarihi
        public DateTime? UpdatedDate { get; set; }

        // Sepetin toplam tutarı (hesaplama kolaylığı için eklenebilir)
        [Column(TypeName = "Decimal(18,2)")]
        public decimal TotalPrice { get; set; } = 0;

        // Sepetteki ürünler
        public virtual ICollection<BasketItem> CartItems { get; set; } = new List<BasketItem>();
    }
}
