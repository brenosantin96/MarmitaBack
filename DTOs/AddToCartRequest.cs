namespace MarmitaBackend.DTOs
{
    public class AddToCartRequest
    {
        public int? KitId { get; set; }
        public int? LunchboxId { get; set; }
        public int Quantity { get; set; }
    }
}

//Esse DTO separado AddToCartRequest: evita problemas de binding ao tentar passar Kit e Lunchbox diretamente no FromBody.
