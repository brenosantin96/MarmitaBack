using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Net;
using System.Text.Json.Serialization;

namespace MarmitaBackend.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;



        public int CartId { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public Cart Cart { get; set; } = null!;

        public int DeliveryInfoId { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public DeliveryInfo DeliveryInfo { get; set; } = null!; // 1:1

        public int PaymentMethodId { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public PaymentMethod PaymentMethod { get; set; } = null!; //1:1

        // Informações fiscais
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;

        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }

        public DateTime CreatedAt { get; set; }
    }

}
