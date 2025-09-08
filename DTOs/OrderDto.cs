namespace MarmitaBackend.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public DeliveryInfoDto DeliveryInfo { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
    }


}
