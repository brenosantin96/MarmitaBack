using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;

namespace MarmitaBackend.Models
{
    public class Address
    {
        public int Id { get; set; }

        public int TenantId { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public Tenant Tenant { get; set; } = null!;

        public int UserId { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public User User { get; set; } = null!;
        public string ZipCode { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Neighborhood { get; set; }
        public string Number { get; set; }
        public string Complement { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
