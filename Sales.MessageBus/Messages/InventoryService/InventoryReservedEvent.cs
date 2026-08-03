namespace Sales.MessageBus.Messages.InventoryService
{
    public class InventoryReservedEvent
    {
        public Guid OrderId { get; set; }
        public bool Success { get; set; }
        public int CustomerId { get; set; }
        public string PaymentMethod { get; set; }
        public decimal Amount { get; set; }
    }
}
