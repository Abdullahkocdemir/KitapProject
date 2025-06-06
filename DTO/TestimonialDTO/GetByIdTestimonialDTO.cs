namespace KitapProject.DTO.TestimonialDTO
{
    public class GetByIdTestimonialDTO
    {
        public int TestimonialId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Rating { get; set; }
    }
}
