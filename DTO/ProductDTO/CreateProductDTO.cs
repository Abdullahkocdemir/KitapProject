namespace KitapProject.DTO.ProductDTO
{
    public class CreateProductDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IFormFile? ImageFile { get; set; } // Dosya yükleme için IFormFile
        public decimal Price { get; set; }
        public bool Status { get; set; }
        public bool PopulerProduct { get; set; }
        public int CategoryId { get; set; }
    }
}
