namespace Sales.MessageBus.Messages.InventoryService
{
    public class InventoryNotReservedEvent
    {
        public Guid OrderId { get; set; }
        public bool Success {  get; set; }
        public string Message { get; set; }
    }
}
