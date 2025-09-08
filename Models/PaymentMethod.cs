﻿namespace MarmitaBackend.Models
{
    public class PaymentMethod
    {
        public int Id { get; set; }

        // Nome visível para o usuário (ex: "PIX", "Cartão de Crédito")
        public string Name { get; set; } = null!;

        // Opcional: descrição ou instruções (ex: "Escaneie o QR code")
        public string? Description { get; set; }

        // Relacionamento reverso (opcional)
        public ICollection<Order>? Orders { get; set; }
    }

}
