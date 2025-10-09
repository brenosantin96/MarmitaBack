namespace MarmitaBackend.DTOs //Data transfer object
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int? KitId { get; set; }
        public int? LunchboxId { get; set; }
        public string? Name { get; set; } 
        public decimal? Price { get; set; }

        public int? PortionGram { get; set; }
        public string? ImageUrl { get; set; }


    }
}

