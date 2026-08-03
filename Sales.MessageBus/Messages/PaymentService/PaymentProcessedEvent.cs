namespace Sales.MessageBus.Messages.PaymentService
{
    public class PaymentProcessedEvent
    {
        public Guid OrderId { get; set; }
        public bool Success { get; set; }
    }
}