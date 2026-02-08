using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;

namespace MarmitaBackend.Models
{
    public class DeliveryInfo
    {
        public int Id { get; set; }

        public int TenantId { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public Tenant Tenant { get; set; } = null!;

        public int CartId { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public Cart Cart { get; set; } = null!;



        public int UserId { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public User User { get; set; } = null!;


        public int? AddressId { get; set; }
        public Address? Address { get; set; }

        public string DeliveryType { get; set; } = null!;  // "Entrega" ou "Retirada"

        public DateTime? DeliveryDate { get; set; }

        public string? DeliveryPeriod { get; set; } // "manhã", "tarde", "noite"

        public bool CanLeaveAtDoor { get; set; }

        // Relacionamento reverso com Order (1 para 1)
        public Order? Order { get; set; }
    }
}
