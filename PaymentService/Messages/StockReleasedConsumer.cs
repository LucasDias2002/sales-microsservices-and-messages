using Sales.MessageBus;
using Sales.MessageBus.Messages.InventoryService;
using Sales.PaymentService.DTOs;

namespace Sales.PaymentService.Services
{
    public class StockReleasedConsumer : BackgroundService
    {
        private readonly IRabbitMQConsumer _consumer;
        private readonly ILogger<StockReleasedConsumer> _logger;

        public StockReleasedConsumer(
            IRabbitMQConsumer consumer,
            ILogger<StockReleasedConsumer> logger)
        {
            _consumer = consumer;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "StockReleasedConsumer iniciado. Aguardando eventos de estoque liberado...");

            _consumer.ConsumeAsync<ReleasedStockEvent>(
                exchange: "ecommerceEvents",
                queue: "payment_stock_released_queue",
                routingKey: "stock.released",
                handler: async (releasedStockEvent) =>
                {
                    _logger.LogInformation(
                        "Evento de estoque liberado recebido. OrderId: {OrderId}",
                        releasedStockEvent.OrderId);

                    var paymentDto = new PaymentProcessed
                    {
                        OrderId = releasedStockEvent.OrderId
                    };

                    _logger.LogInformation(
                        "Iniciando cancelamento do pagamento. OrderId: {OrderId}",
                        paymentDto.OrderId);

                    var cancelled = await CancellPayment();

                    if (cancelled)
                    {
                        _logger.LogInformation(
                            "Pagamento cancelado com sucesso. OrderId: {OrderId}",
                            paymentDto.OrderId);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Não foi possível cancelar o pagamento. OrderId: {OrderId}",
                            paymentDto.OrderId);
                    }
                });

            return Task.CompletedTask;
        }

        // Simulate payment cancellation logic
        private async Task<bool> CancellPayment()
        {
            _logger.LogDebug("Executando lógica de cancelamento do pagamento...");

            await Task.CompletedTask;

            return true;
        }
    }
}