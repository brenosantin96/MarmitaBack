namespace MarmitaBackend.DTOs //Data transfer object
{
    public class CartDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CartItemDto> Items { get; set; }
        public bool isCheckedOut { get; set; }
    }

}
