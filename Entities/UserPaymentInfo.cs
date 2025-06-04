using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KitapProject.Entities
{
    public class UserPaymentInfo
    {
        [Key]
        public int PaymentInfoId { get; set; }

        [Required]
        [ForeignKey("AppUser")]
        public string AppUserId { get; set; } = string.Empty;
        public virtual AppUser AppUser { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string CardHolderName { get; set; } = string.Empty;

        [Required]
        [StringLength(19)] // Kart numarası 16 haneli + 3 boşluk
        public string CardNumberLastFour { get; set; } = string.Empty; // Güvenlik için sadece son 4 haneyi saklayalım

        [Required]
        [StringLength(5)] // MM/YY
        public string ExpirationDate { get; set; } = string.Empty;

        // Gerçek bir uygulamada CVV saklanmaz. Bu sadece örnek içindir.
        [Required]
        [StringLength(4)]
        public string CVV { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Varsayılan Kart")]
        public bool IsDefault { get; set; } = false;
    }
}