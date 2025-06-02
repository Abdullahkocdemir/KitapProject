using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KitapProject.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required]
        [Column(TypeName = "VarChar")]
        [StringLength(15)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "VarChar")]
        [StringLength(100)]
        public string Description { get; set; } = string.Empty;
        [Required]
        [Column(TypeName = "VarChar")]
        [StringLength(20)]
        public string Icon { get; set; } = string.Empty;
    }
}
