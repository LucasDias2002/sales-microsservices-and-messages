using Sales.OrderService.Entities.Enums;

namespace Sales.OrderService.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public OrderStatus Status { get; set; }
        public OrderPaymentMethod PaymentMethod { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
