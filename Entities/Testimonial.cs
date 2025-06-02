using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KitapProject.Entities
{
    public class Testimonial
    {
        public int TestimonialId { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "VarChar")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "VarChar")]
        [StringLength(100)]
        public string Description { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }
    }
}
