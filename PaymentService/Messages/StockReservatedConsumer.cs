using Sales.MessageBus;
using Sales.MessageBus.Messages.InventoryService;
using Sales.MessageBus.Messages.PaymentService;
using Sales.PaymentService.DTOs;

namespace Sales.PaymentService.Messages
{
    public class StockReservatedConsumer: BackgroundService
    {
        private readonly IRabbitMQConsumer _consumer;
        private readonly IRabbitMQPublisher _publisher;
        public StockReservatedConsumer(IRabbitMQConsumer consumer, IRabbitMQPublisher publisher)
        {
            _consumer = consumer;
            _publisher = publisher;
        }   

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.ConsumeAsync<InventoryReservedEvent>(
                exchange: "ecommerceEvents",
                queue: "payment_stock_reserved_queue",
                routingKey: "stock.reserved",
                handler: async (reservationEvent) =>
                {
                    var payment = new PaymentProcessed
                    {
                        OrderId = reservationEvent.OrderId,
                        CustomerId = reservationEvent.CustomerId,
                        Amount = reservationEvent.Amount,
                        PaymentDate = DateTime.UtcNow
                    };

                    await ProcessPayment(payment);
                });
            return Task.CompletedTask;
        }
        private async Task ProcessPayment(PaymentProcessed payment)
        {
            if (payment.Amount > 0 && payment.Amount < 1000)
            {
                await _publisher.Publish<PaymentProcessedEvent>(new PaymentProcessedEvent
                {
                    OrderId = payment.OrderId,
                    Success = true
                }, "ecommerceEvents", "payment.processed");
                return;
            }

            await _publisher.Publish<PaymentProcessedEvent>(new PaymentProcessedEvent
            {
                OrderId = payment.OrderId,
                Success = false
            }, "ecommerceEvents", "payment.processed");
        }
    }
}
