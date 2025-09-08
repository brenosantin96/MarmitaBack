namespace MarmitaBackend.DTOs
{
    public class DeliveryInfoDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public int? AddressId { get; set; }
        public string DeliveryType { get; set; } = null!;
        public DateTime DeliveryDate { get; set; }
        public string DeliveryPeriod { get; set; } = null!;
        public bool CanLeaveAtDoor { get; set; }

    }
}
