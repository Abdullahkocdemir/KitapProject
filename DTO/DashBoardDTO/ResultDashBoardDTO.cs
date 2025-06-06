namespace KitapProject.DTO.DashBoardDTO
{
    public class ResultDashBoardDTO
    {
        public int TotalBooks { get; set; }
        public int TotalCategories { get; set; }
        public int TotalTestimonials { get; set; }
        public int TotalUsers { get; set; }

        public List<LastAddedBookDTO> LastAddedBooks { get; set; } = new List<LastAddedBookDTO>();


        public List<int> TestimonialRatingCounts { get; set; } = new List<int>(); 
        public List<string> TestimonialRatingLabels { get; set; } = new List<string> { "1 Yıldız", "2 Yıldız", "3 Yıldız", "4 Yıldız", "5 Yıldız" };

        public List<string> BooksByCategoryLabels { get; set; } = new List<string>(); 
        public List<int> BooksByCategoryCounts { get; set; } = new List<int>(); 
    }
}
