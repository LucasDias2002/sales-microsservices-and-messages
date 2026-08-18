using Sales.InventoryService.DTOs;
using Sales.MessageBus;
using Sales.MessageBus.Messages.InventoryService;
using Sales.OrderService.DTOs;
using Sales.OrderService.Entities.Enums;
using Sales.OrderService.Services;

namespace Sales.OrderService.Message
{
    public class StockNotReservedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRabbitMQConsumer _consumer;

        public StockNotReservedConsumer(IServiceScopeFactory scopeFactory, IRabbitMQConsumer consumer)
        {
            _scopeFactory = scopeFactory;
            _consumer = consumer;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.ConsumeAsync<InventoryNotReservedEvent>(
                exchange: "ecommerceEvents",
                queue: "order_stock_not_reservation_queue",
                routingKey: "stock.not.reserved",
                handler: async (notReservedEvent) =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var orderService = scope.ServiceProvider
                        .GetRequiredService<IOrderService>();

                    var order = new OrderDTO
                    {
                        Id = notReservedEvent.OrderId,
                        Status = OrderStatus.Cancelled,
                        CancellationReason = notReservedEvent.Message
                    };

                    var result = await orderService.UpdateStatusAsync(order, order.Status);
                });
            return Task.CompletedTask;
        }
    }
}
