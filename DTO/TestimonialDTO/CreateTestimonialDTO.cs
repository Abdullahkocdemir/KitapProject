using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KitapProject.DTO.TestimonialDTO
{
    public class CreateTestimonialDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Rating { get; set; }
    }
}
