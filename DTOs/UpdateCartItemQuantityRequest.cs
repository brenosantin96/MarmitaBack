namespace MarmitaBackend.DTOs
{
    public class UpdateCartItemQuantityRequest
    {
        public int CartItemId { get; set; }
        public int NewQuantity { get; set; }
    }
}
