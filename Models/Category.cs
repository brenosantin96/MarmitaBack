using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;

namespace MarmitaBackend.Models
{
    public class Category
    {
        public int Id { get; set; }

        public int TenantId { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public Tenant Tenant { get; set; } = null!;


        public string Name { get; set; }

        // Relação 1:N com Lunchboxes Cada marmita pertence a uma categoria, e cada categoria pode conter várias marmitas
        public List<Lunchbox> Lunchboxes { get; set; } = new();
    }
}
