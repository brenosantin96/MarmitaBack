using MarmitaBackend.Enums;

namespace MarmitaBackend.DTOs
{
    public class OrderCreateDto
    {
        public int CartId { get; set; }
        public int DeliveryInfoId { get; set; }
        public int PaymentMethodId { get; set; }
        public OrderStatus OrderStatus { get; set; } = OrderStatus.PendingPayment;

        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;

        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }
    }
}
