namespace Sales.InventoryService.DTOs
{
    public class InventoryReservedEvent
    {
        public Guid OrderId { get; set; }
        public int CustomerId { get; set; }
        public bool Success { get; set; }
    }
}
