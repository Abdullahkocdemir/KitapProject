using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace KitapProject.Entities
{
    public class AppUser : IdentityUser
    {
        [Required]
        [Display(Name = "Ad")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Soyad")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Ad Soyad")]
        public string FullName => $"{FirstName} {LastName}";

        [Display(Name = "Aktif Mi?")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Kapanış Tarihi")]
        public DateTime? ClosedAt { get; set; }

        [Display(Name = "Profil Fotoğrafı")]
        public string? ProfilePhotoUrl { get; set; }

        [StringLength(250)]
        [Display(Name = "Varsayılan Gönderim Adresi")]
        public string? ShippingAddress { get; set; }

        [StringLength(50)]
        [Display(Name = "Şehir")]
        public string? City { get; set; }

        [StringLength(50)]
        [Display(Name = "Ülke")]
        public string? Country { get; set; }

        [StringLength(10)]
        [Display(Name = "Posta Kodu")]
        public string? PostalCode { get; set; }

        public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; } = new List<IdentityUserRole<string>>();

        public virtual ICollection<UserPaymentInfo> UserPaymentInfos { get; set; } = new List<UserPaymentInfo>();
    }
}