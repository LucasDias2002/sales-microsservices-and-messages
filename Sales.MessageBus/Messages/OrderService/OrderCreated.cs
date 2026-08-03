namespace Sales.MessageBus.Messages.OrderService
{
    public class OrderCreated
    {
        public Guid Id { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
