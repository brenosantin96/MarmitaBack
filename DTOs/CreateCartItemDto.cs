namespace MarmitaBackend.DTOs
{
    public class CreateCartItemDto
    {
        public int Quantity { get; set; }
        public int? KitId { get; set; }
        public int? LunchboxId { get; set; }

    }
}
