using Sales.InventoryService.DTOs;
using Sales.InventoryService.Services;
using Sales.MessageBus;
using Sales.MessageBus.Messages.InventoryService;
using Sales.MessageBus.Messages.OrderService;
using Sales.MessageBus.Messages.PaymentService;

namespace Sales.InventoryService.Messages
{
    public class OrderCancelledConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRabbitMQConsumer _consumer;
        private readonly IRabbitMQPublisher _publisher;

        public OrderCancelledConsumer(IServiceScopeFactory scopeFactory, IRabbitMQConsumer consumer, IRabbitMQPublisher publisher)
        {
            _scopeFactory = scopeFactory;
            _consumer = consumer;
            _publisher = publisher;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.ConsumeAsync<OrderCreated>(
                exchange: "ecommerceEvents",
                queue: "inventory_order_cancelled_queue",
                routingKey: "order.cancelled",
                handler: async (orderCancelled) =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var productService = scope.ServiceProvider
                        .GetRequiredService<IProductService>();

                    var result = await productService.CancelReservationAsync(orderCancelled.Id);

                    var releasedStockEvent = new ReleasedStockEvent
                    {
                        OrderId = orderCancelled.Id,
                    };

                    await _publisher.Publish<ReleasedStockEvent>(
                        exchange: "ecommerceEvents",
                        routingKey: "stock.released",
                        message: releasedStockEvent);
                    
                });
            return Task.CompletedTask;
        }
    }
    
}
