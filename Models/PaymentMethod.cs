using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;

namespace MarmitaBackend.Models
{
    public class PaymentMethod
    {
        public int Id { get; set; }

        public int TenantId { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public Tenant Tenant { get; set; } = null!;



        // Nome visível para o usuário (ex: "PIX", "Cartão de Crédito")
        public string Name { get; set; } = null!;

        // Opcional: descrição ou instruções (ex: "Escaneie o QR code")
        public string? Description { get; set; }

        // Relacionamento reverso (opcional)
        public ICollection<Order>? Orders { get; set; }
    }

}
