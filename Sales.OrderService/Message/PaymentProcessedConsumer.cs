using Sales.MessageBus;
using Sales.MessageBus.Messages.PaymentService;
using Sales.OrderService.DTOs;
using Sales.OrderService.Entities.Enums;
using Sales.OrderService.Services;

namespace Sales.OrderService.Message
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
                queue: "order_payment_processed_queue",
                routingKey: "payment.processed",
                handler: async (paymentProcessed) =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var orderService = scope.ServiceProvider
                        .GetRequiredService<IOrderService>();

                    var order = new OrderDTO { Id = paymentProcessed.OrderId };

                    if(paymentProcessed.Success)
                        await orderService.UpdateStatusAsync(order, OrderStatus.Confirmed);
                    else
                    {
                        order.CancellationReason = "Pagamento recusado";
                        await orderService.UpdateStatusAsync(order, OrderStatus.Cancelled);
                    }

                });
            return Task.CompletedTask;
        }
    }
}
