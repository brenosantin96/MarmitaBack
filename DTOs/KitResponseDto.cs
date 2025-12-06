namespace MarmitaBackend.DTOs
{
    public class KitResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public List<int> LunchboxIds { get; set; }
    }

}
