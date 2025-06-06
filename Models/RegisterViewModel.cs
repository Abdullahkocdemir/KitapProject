using System.ComponentModel.DataAnnotations;

namespace KitapProject.Models 
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olmalıdır.")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olmalıdır.")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kullanıcı adı alanı zorunludur.")]
        [StringLength(256, ErrorMessage = "{0} en fazla {1} karakter olmalıdır.", MinimumLength = 3)]
        [Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        // BURASI GÜNCELLENDİ: PhoneNumber'ı string? olarak değiştiriyoruz
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")] // Telefon formatı doğrulaması
        [StringLength(20, ErrorMessage = "Telefon numarası en fazla 20 karakter olmalıdır.")]
        [Display(Name = "Telefon Numarası")]
        public string? PhoneNumber { get; set; } // Nullable string olarak tanımlandı

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "{0} en az {2} ve en fazla {1} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Şifreyi Onayla")]
        [Compare("Password", ErrorMessage = "Şifre ve onay şifresi eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}