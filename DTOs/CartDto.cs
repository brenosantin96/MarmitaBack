namespace MarmitaBackend.DTOs
{
    public class CartDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CartItemDto> Items { get; set; }
    }

}
