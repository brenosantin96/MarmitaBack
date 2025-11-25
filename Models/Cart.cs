using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MarmitaBackend.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        // Relacionamento com o User (asp.net Identity geralmente usa string como ID)
        public int UserId { get; set; }

        // Propriedade de navegação
        [ValidateNever]
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public bool IsCheckedOut { get; set; }

        // Lista de itens no carrinho
        public List<CartItem> CartItems { get; set; } = new();

    }
}
