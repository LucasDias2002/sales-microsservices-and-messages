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
                queue: "order_payment_processed_queue",
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

                        var orderService = scope.ServiceProvider
                            .GetRequiredService<IOrderService>();

                        var order = new OrderDTO
                        {
                            Id = paymentProcessed.OrderId
                        };

                        if (paymentProcessed.Success)
                        {
                            _logger.LogInformation(
                                "Pagamento aprovado. Confirmando pedido. OrderId: {OrderId}",
                                paymentProcessed.OrderId);

                            await orderService.UpdateStatusAsync(
                                order,
                                OrderStatus.Confirmed);

                            _logger.LogInformation(
                                "Pedido confirmado com sucesso. OrderId: {OrderId}",
                                paymentProcessed.OrderId);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Pagamento recusado. Cancelando pedido. OrderId: {OrderId}",
                                paymentProcessed.OrderId);

                            order.CancellationReason = "Pagamento recusado";

                            await orderService.UpdateStatusAsync(
                                order,
                                OrderStatus.Cancelled);

                            _logger.LogInformation(
                                "Pedido cancelado com sucesso. OrderId: {OrderId}, Motivo: {Reason}",
                                paymentProcessed.OrderId,
                                order.CancellationReason);
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