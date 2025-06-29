namespace MarmitaBackend.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Relação 1:N com Lunchboxes Cada marmita pertence a uma categoria, e cada categoria pode conter várias marmitas
        public List<Lunchbox> Lunchboxes { get; set; } = new();
    }
}
