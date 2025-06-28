namespace MarmitaBackend.Models
{
    public class Kit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        //Marmitas incluidas neste kit
        public List<KitLunchBox> KitLunchBoxes { get; set; }
    }
}
