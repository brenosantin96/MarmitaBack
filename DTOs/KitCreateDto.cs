namespace MarmitaBackend.DTOs
{
    public class KitCreateDto
    {
        
        public string Name { get; set; }        
        public string Description { get; set; }        
        public decimal Price { get; set; }        
        public int CategoryId { get; set; }
        public IFormFile Image { get; set; }
        public List<int>? LunchboxIds { get; set; } // opcional, se quiser criar já com marmitas
    }

}
