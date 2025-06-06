namespace KitapProject.DTO.ProductDTO
{
    public class UpdateProductDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IFormFile? ImageFile { get; set; } 
        public string? CurrentImageUrl { get; set; } = string.Empty; 
        public DateTime? UpdatedDate { get; set; }
        public bool Status { get; set; }
        public bool PopulerProduct { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
    }
}
