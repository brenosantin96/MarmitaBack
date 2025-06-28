namespace MarmitaBackend.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; set; }
        public Cart Cart { get; set; } = null!;

        //Produto individual
        public int? LunchboxId { get; set; }
        public Lunchbox? Lunchbox { get; set; }

        // Kit de produtos
        public int? KitId { get; set; }
        public Kit? Kit { get; set; }

        public int Quantity { get; set; }
    }
}

//OBS: Apenas um dos campos (LunchboxId ou KitId) deve ser preenchido em cada CartItem.