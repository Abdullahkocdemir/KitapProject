namespace KitapProject.DTO.ProductDTO
{
    public class ResultProductDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageURl { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool Status { get; set; }
        public bool PopulerProduct { get; set; }
        public decimal Price { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}
