namespace MarmitaBackend.Models
{
    public class KitLunchbox
    {
        public int KitId { get; set; }
        public Kit Kit { get; set; }
        public int LunchBoxId { get; set; }
        public Lunchbox Lunchbox { get; set; }
        public int Quantity { get; set; } // Quantidade da marmita neste kit

    }
}
