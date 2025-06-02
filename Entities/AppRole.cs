using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace KitapProject.Entities
{
    public class AppRole : IdentityRole
    {
        [Required]
        [Display(Name = "Rol Açıklaması")]
        [StringLength(256)]
        public string Description { get; set; } = string.Empty;
        public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; } = new List<IdentityUserRole<string>>();
    }
}
