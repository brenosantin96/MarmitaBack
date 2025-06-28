namespace MarmitaBackend.Models
{
    public class Cart
    {
        public int Id { get; set; }

        // Relacionamento com o User (asp.net Identity geralmente usa string como ID)
        public int UserId { get; set; }

        // Propriedade de navegação
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public bool IsCheckedOut { get; set; }

        // Lista de itens no carrinho
        public List<CartItem> CartItems { get; set; } = new();

    }
}
