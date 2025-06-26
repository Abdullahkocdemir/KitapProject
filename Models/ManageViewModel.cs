using System.ComponentModel.DataAnnotations;

namespace KitapProject.Models
{
    public class ManageViewModel
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public IList<string>? Roles { get; set; }

        [Required(ErrorMessage = "Mevcut şifrenizi girmeniz gereklidir.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifre belirlemeniz gereklidir.")]
        [StringLength(100, ErrorMessage = "{0} en az {2} ve en fazla {1} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre (Tekrar)")]
        [Compare("NewPassword", ErrorMessage = "Yeni şifre ile şifre tekrarı uyuşmuyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
