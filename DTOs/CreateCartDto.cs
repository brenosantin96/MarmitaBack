namespace MarmitaBackend.DTOs //Data transfer object
{
    public class CreateCartDto
    {
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool isCheckedOut { get; set; }
        public List<CreateCartItemDto> CartItems { get; set; }
    }

}
