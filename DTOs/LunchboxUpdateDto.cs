namespace MarmitaBackend.DTOs
{
    public class LunchboxUpdateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        public int PortionGram { get; set; }

        public int CategoryId { get; set; }

        // opcional: pode ou não vir uma nova imagem
        public IFormFile? Image { get; set; }
    }
}
