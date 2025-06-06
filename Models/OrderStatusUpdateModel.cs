namespace KitapProject.Models
{
    public class OrderStatusUpdateModel
    {
        public int OrderId { get; set; }
        public string NewStatus { get; set; } = string.Empty;
    }
}
