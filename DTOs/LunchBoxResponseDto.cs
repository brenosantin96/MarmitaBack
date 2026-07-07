namespace MarmitaBackend.DTOs
{
    public class LunchboxResponseDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int PortionGram { get; set; }
        public int CategoryId { get; set; }
    }

}
