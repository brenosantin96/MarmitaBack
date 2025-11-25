namespace MarmitaBackend.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;


        public int CartId { get; set; }
        public Cart Cart { get; set; } = null!;

        //Produto individual
        public int? LunchboxId { get; set; } // Pode ser nulo se for um kit
        public Lunchbox? Lunchbox { get; set; } 

        // Kit de produtos
        public int? KitId { get; set; } // Pode ser nulo se for uma marmita individual
        public Kit? Kit { get; set; }

        public int Quantity { get; set; }
    }
}

//OBS: Apenas um dos campos (LunchboxId ou KitId) deve ser preenchido em cada CartItem.