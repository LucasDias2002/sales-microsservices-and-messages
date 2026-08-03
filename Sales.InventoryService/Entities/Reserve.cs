using Sales.InventoryService.Entities.Enum;

namespace Sales.InventoryService.Entities
{
    public class Reserve
    {
        public Guid Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public Guid OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public ReservationStatus Status { get; set; }
    }
}
