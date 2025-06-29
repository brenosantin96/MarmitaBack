namespace MarmitaBackend.Models
{
    public class Kit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;


        //Marmitas incluidas neste kit
        public List<KitLunchbox> KitLunchboxes { get; set; }
    }
}
