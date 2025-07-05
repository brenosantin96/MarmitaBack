namespace MarmitaBackend.DTOs
{
    public class RemoveFromCartRequest
    {
        public int CartItemId { get; set; }
        public int Quantity { get; set; }
    }
}

