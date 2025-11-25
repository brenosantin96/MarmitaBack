namespace MarmitaBackend.Models
{
    public class User
    {
        public int Id { get; set; }

        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;



        public string Name { get; set; }
        public string Email { get; set; }
        public string? Password { get; set; }
        public bool isAdmin { get; set; }

        // Relacionamento 1:N (User → Addresses)
        public ICollection<Address> Addresses { get; set; } = new List<Address>(); //inicializa vazio
         
        // Relacionamento 1:N (User → DeliveryInfos)
        public ICollection<DeliveryInfo> DeliveryInfos { get; set; } = new List<DeliveryInfo>();
    }
}
