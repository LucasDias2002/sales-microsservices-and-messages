using RabbitMQ.Client;
using Sales.OrderService.Entities;
using Sales.OrderService.Entities.Enums;
using System.Data;

namespace Sales.OrderService.DTOs
{
    public class OrderDTO
    {
        public Guid Id { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } 
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public OrderPaymentMethod PaymentMethod { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
