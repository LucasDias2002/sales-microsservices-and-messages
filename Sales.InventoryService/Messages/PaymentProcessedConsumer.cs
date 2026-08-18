using Sales.InventoryService.Services;
using Sales.MessageBus;
using Sales.MessageBus.Messages.PaymentService;

namespace Sales.InventoryService.Messages
{
    public class PaymentProcessedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRabbitMQConsumer _consumer;
        private readonly ILogger<PaymentProcessedConsumer> _logger;

        public PaymentProcessedConsumer(
            IServiceScopeFactory scopeFactory,
            IRabbitMQConsumer consumer,
            ILogger<PaymentProcessedConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _consumer = consumer;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "PaymentProcessedConsumer iniciado. Aguardando eventos payment.processed...");

            _consumer.ConsumeAsync<PaymentProcessedEvent>(
                exchange: "ecommerceEvents",
                queue: "inventory_payment_processed_queue",
                routingKey: "payment.processed",
                handler: async (paymentProcessed) =>
                {
                    _logger.LogInformation(
                        "Evento payment.processed recebido. OrderId: {OrderId}, Success: {Success}",
                        paymentProcessed.OrderId,
                        paymentProcessed.Success);

                    try
                    {
                        using var scope = _scopeFactory.CreateScope();

                        var productService = scope.ServiceProvider
                            .GetRequiredService<IProductService>();

                        if (paymentProcessed.Success)
                        {
                            _logger.LogInformation(
                                "Pagamento aprovado. Confirmando reserva de estoque. OrderId: {OrderId}",
                                paymentProcessed.OrderId);

                            await productService
                                .ConfirmReservationAsync(paymentProcessed.OrderId);

                            _logger.LogInformation(
                                "Reserva de estoque confirmada com sucesso. OrderId: {OrderId}",
                                paymentProcessed.OrderId);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Pagamento recusado. Cancelando reserva de estoque. OrderId: {OrderId}",
                                paymentProcessed.OrderId);

                            await productService
                                .CancelReservationAsync(paymentProcessed.OrderId);

                            _logger.LogInformation(
                                "Reserva de estoque cancelada com sucesso. OrderId: {OrderId}",
                                paymentProcessed.OrderId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Erro ao processar payment.processed. OrderId: {OrderId}",
                            paymentProcessed.OrderId);
                    }
                });

            return Task.CompletedTask;
        }
    }
}