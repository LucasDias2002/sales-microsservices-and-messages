namespace Sales.InventoryService.DTOs
{
    public class InventoryReservedEvent
    {
        public Guid OrderId { get; set; }
        public bool Success { get; set; }
    }
}
