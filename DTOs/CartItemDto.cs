namespace MarmitaBackend.DTOs //Data transfer object
{
    public class CartItemDto
    {
        public int CartItemId { get; set; }
        public int Quantity { get; set; }
        public int? KitId { get; set; }
        public int? LunchboxId { get; set; }
        public string? ProductName { get; set; } 
    }
}

