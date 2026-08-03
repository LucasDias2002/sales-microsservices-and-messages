using Sales.InventoryService.Services;
using Sales.MessageBus;
using Sales.MessageBus.Messages.PaymentService;

namespace Sales.InventoryService.Messages
{
    public class PaymentProcessedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRabbitMQConsumer _consumer;

        public PaymentProcessedConsumer(IServiceScopeFactory scopeFactory, IRabbitMQConsumer consumer)
        {
            _scopeFactory = scopeFactory;
            _consumer = consumer;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.ConsumeAsync<PaymentProcessedEvent>(
                exchange: "ecommerceEvents",
                queue: "inventory_payment_processed_queue",
                routingKey: "payment.processed",
                handler: async (paymentProcessed) =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var productService = scope.ServiceProvider
                        .GetRequiredService<IProductService>();
                    if(paymentProcessed.Success)
                        await productService.ConfirmReservationAsync(paymentProcessed.OrderId);
                    else
                        await productService.CancelReservationAsync(paymentProcessed.OrderId);

                });
            return Task.CompletedTask;
        }
    }
}
