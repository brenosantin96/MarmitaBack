using System.Net;

namespace MarmitaBackend.Models
{
    public class Order
    {
        public int Id { get; set; }

        // Carrinho associado (obrigatório)
        public int CartId { get; set; }
        public Cart Cart { get; set; } = null!;

        // Endereço associado (opcional, só se for entrega)
        public int? AddressId { get; set; }
        public Address? Address { get; set; }

        // "Entrega" ou "Retirada"
        public string DeliveryType { get; set; } = null!;

        // Data da entrega ou retirada
        public DateTime DeliveryDate { get; set; }

        // "Manhã", "Tarde", "Noite"
        public string DeliveryPeriod { get; set; } = null!;

        // Flag para portaria
        public bool CanLeaveAtDoor { get; set; }

        // Informações para nota fiscal                  
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;

        // Método de pagamento, ex: "PIX"
        public string PaymentMethod { get; set; } = null!;

        // Valores financeiros
        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; } //Frete
        public decimal Total { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
