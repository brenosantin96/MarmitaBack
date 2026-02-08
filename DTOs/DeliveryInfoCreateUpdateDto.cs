namespace MarmitaBackend.DTOs
{
    public class DeliveryInfoCreateUpdateDto
    {
        public int CartId { get; set; }
        public int? AddressId { get; set; }

        public string DeliveryType { get; set; } = null!; // "Entrega" ou "Retirada"

        public DateTime? DeliveryDate { get; set; }

        public string? DeliveryPeriod { get; set; } = null!; // "manhã", "tarde", "noite"

        public bool CanLeaveAtDoor { get; set; }

    }
}
