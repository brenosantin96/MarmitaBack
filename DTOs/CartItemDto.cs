namespace MarmitaBackend.DTOs
{
    public class CartItemDto
    {
        public int Quantity { get; set; }
        public int? KitId { get; set; }
        public int? LunchboxId { get; set; }
        public string? ProductName { get; set; } 
    }
}
