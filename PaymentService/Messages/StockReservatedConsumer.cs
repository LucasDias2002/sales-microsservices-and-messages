using Sales.MessageBus;
using Sales.MessageBus.Messages.InventoryService;
using Sales.MessageBus.Messages.PaymentService;
using Sales.PaymentService.DTOs;

namespace Sales.PaymentService.Messages
{
    public class StockReservatedConsumer : BackgroundService
    {
        private readonly IRabbitMQConsumer _consumer;
        private readonly IRabbitMQPublisher _publisher;
        private readonly ILogger<StockReservatedConsumer> _logger;

        public StockReservatedConsumer(
            IRabbitMQConsumer consumer,
            IRabbitMQPublisher publisher,
            ILogger<StockReservatedConsumer> logger)
        {
            _consumer = consumer;
            _publisher = publisher;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Payment Service iniciado. Aguardando eventos de estoque reservado...");

            _consumer.ConsumeAsync<InventoryReservedEvent>(
                exchange: "ecommerceEvents",
                queue: "payment_stock_reserved_queue",
                routingKey: "stock.reserved",
                handler: async (reservationEvent) =>
                {
                    _logger.LogInformation(
                        "Evento de estoque reservado recebido. OrderId: {OrderId}, CustomerId: {CustomerId}, Amount: {Amount}",
                        reservationEvent.OrderId,
                        reservationEvent.CustomerId,
                        reservationEvent.Amount);

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
            _logger.LogInformation(
                "Processando pagamento. OrderId: {OrderId}, Amount: {Amount}",
                payment.OrderId,
                payment.Amount);

            var success = payment.Amount > 0 && payment.Amount < 1000;

            if (success)
            {
                _logger.LogInformation(
                    "Pagamento aprovado. OrderId: {OrderId}, Amount: {Amount}",
                    payment.OrderId,
                    payment.Amount);
            }
            else
            {
                _logger.LogWarning(
                    "Pagamento recusado. OrderId: {OrderId}, Amount: {Amount}",
                    payment.OrderId,
                    payment.Amount);
            }

            await _publisher.Publish<PaymentProcessedEvent>(
                new PaymentProcessedEvent
                {
                    OrderId = payment.OrderId,
                    Success = success
                },
                "ecommerceEvents",
                "payment.processed");

            _logger.LogInformation(
                "Evento payment.processed publicado. OrderId: {OrderId}, Success: {Success}",
                payment.OrderId,
                success);
        }
    }
}