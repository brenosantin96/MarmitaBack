namespace MarmitaBackend.Models
{
    public class KitLunchbox
    {
        public int Id { get; set; }

        public int KitId { get; set; }
        public Kit Kit { get; set; }


        public int LunchBoxId { get; set; }
        public Lunchbox Lunchbox { get; set; }


        public int Quantity { get; set; } // Quantidade da marmita neste kit

    }
}

//Essa classe é a tabela de junção entre Kit e Lunchbox
//Contém a propriedade Quantity, que diz quantas vezes essa marmita aparece no kit

