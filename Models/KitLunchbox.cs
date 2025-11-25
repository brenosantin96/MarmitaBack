namespace MarmitaBackend.Models
{
    public class KitLunchbox
    {
        public int Id { get; set; }

        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;



        public int KitId { get; set; }
        public Kit Kit { get; set; }


        public int LunchBoxId { get; set; }
        public Lunchbox Lunchbox { get; set; }


        public int Quantity { get; set; } // Quantidade da marmita neste kit

    }
}

//Essa classe é a tabela de junção entre Kit e Lunchbox
//Contém a propriedade Quantity, que diz quantas vezes essa marmita aparece no kit
//Essa KitLunchbox é a entidade intermediária que liga uma marmita (Lunchbox) a um kit (Kit) e diz quantas vezes essa marmita aparece no kit.