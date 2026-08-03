using Sales.MessageBus;
using Sales.MessageBus.Messages.InventoryService;
using Sales.OrderService.DTOs;
using Sales.OrderService.Entities.Enums;
using Sales.OrderService.Services;

namespace Sales.OrderService.Message
{
    public class StockReservedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRabbitMQConsumer _consumer;

        public StockReservedConsumer(IServiceScopeFactory scopeFactory, IRabbitMQConsumer consumer)
        {
            _scopeFactory = scopeFactory;
            _consumer = consumer;
        }   

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.ConsumeAsync<InventoryReservedEvent>(
                exchange: "ecommerceEvents",
                queue: "order_stock_reserved_queue",
                routingKey: "stock.reserved",
                handler: async (reservationEvent) =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var orderService = scope.ServiceProvider
                        .GetRequiredService<IOrderService>();

                    var order = new OrderDTO
                    {
                        Id = reservationEvent.OrderId,
                        Status = OrderStatus.AwaitingPayment,
                    };

                    var result = await orderService.UpdateStatusAsync(order, order.Status);
                });
            return Task.CompletedTask;
        }
    }
}
