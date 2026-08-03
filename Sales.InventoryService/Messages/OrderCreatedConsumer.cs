using Sales.InventoryService.DTOs;
using Sales.InventoryService.Services;
using Sales.MessageBus;
using Sales.MessageBus.Messages.InventoryService;
using Sales.MessageBus.Messages.OrderService;
using Sales.MessageBus.Messages.PaymentService;

namespace Sales.InventoryService.Messages
{
    public class OrderCreatedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRabbitMQConsumer _consumer;
        private readonly IRabbitMQPublisher _publisher;

        public OrderCreatedConsumer(IServiceScopeFactory scopeFactory, IRabbitMQConsumer consumer, IRabbitMQPublisher publisher)
        {
            _scopeFactory = scopeFactory;
            _consumer = consumer;
            _publisher = publisher;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.ConsumeAsync<OrderCreated>(
                exchange: "ecommerceEvents",
                queue: "inventory_order_created_queue",
                routingKey: "order.created",
                handler: async (orderCreated) =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var productService = scope.ServiceProvider
                        .GetRequiredService<IProductService>();

                    var product = await productService.GetProductByIdAsync(orderCreated.ProductId);

                    if (product != null)
                    {
                        var result = await productService.ReserveStockAsync(orderCreated);
                            
                        if (result) { 
                            var inventoryReservedEvent = new InventoryReservedEvent
                            {
                                OrderId = orderCreated.Id,
                                Success = result,
                                CustomerId = orderCreated.CustomerId,
                                PaymentMethod = orderCreated.PaymentMethod,
                                Amount = orderCreated.Quantity * product.Price
                            };

                            await _publisher.Publish<InventoryReservedEvent>(
                                exchange: "ecommerceEvents",
                                routingKey: "stock.reserved",
                                message: inventoryReservedEvent);
                        }
                        else
                        {
                            var notReserved = new InventoryNotReservedEvent
                            {
                                OrderId = orderCreated.Id,
                                Success = false,
                                Message = "Insufficient stock"
                            };

                            await _publisher.Publish<InventoryNotReservedEvent>(
                                    exchange: "ecommerceEvents",
                                    routingKey: "stock.not.reserved",
                                    message: notReserved);
                        }

                    }
                });
            return Task.CompletedTask;
        }
    }
    
}
