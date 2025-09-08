namespace MarmitaBackend.DTOs
{
    public class LunchboxCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        public int PortionGram { get; set; }
        public int CategoryId { get; set; }

        // Aqui vem a imagem enviada no form-data
        public IFormFile Image { get; set; }

    }
}
