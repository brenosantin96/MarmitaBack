namespace MarmitaBackend.DTOs
{
    public class KitUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? CategoryId { get; set; }
        public IFormFile? Image { get; set; }

        // Se enviado atualiza completamente as associações
        // Se não enviado não mexe nos LunchboxIds do kit
        public List<int>? LunchboxIds { get; set; }
    }
}
