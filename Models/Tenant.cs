namespace MarmitaBackend.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Domain { get; set; } //ex: cliente1.marmita.com

        public DateTime CreatedAt { get; set; }

        // Relacionamento (cada tenant possui vários usuários e dados)
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
