using MarmitaBackend.Models;

namespace MarmitaBackend.Models
{
    public class Lunchbox
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }

        public List<KitLunchBox> KitLunchboxes { get; set; } = new();
    }
}
