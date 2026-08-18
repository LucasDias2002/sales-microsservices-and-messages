using Sales.InventoryService.Services;
using Sales.MessageBus;
using Sales.MessageBus.Messages.InventoryService;
using Sales.MessageBus.Messages.OrderService;

namespace Sales.InventoryService.Messages
{
    public class OrderCancelledConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRabbitMQConsumer _consumer;
        private readonly IRabbitMQPublisher _publisher;
        private readonly ILogger<OrderCancelledConsumer> _logger;

        public OrderCancelledConsumer(
            IServiceScopeFactory scopeFactory,
            IRabbitMQConsumer consumer,
            IRabbitMQPublisher publisher,
            ILogger<OrderCancelledConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _consumer = consumer;
            _publisher = publisher;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "OrderCancelledConsumer iniciado. Aguardando eventos order.cancelled...");

            _consumer.ConsumeAsync<OrderCreated>(
                exchange: "ecommerceEvents",
                queue: "inventory_order_cancelled_queue",
                routingKey: "order.cancelled",
                handler: async (orderCancelled) =>
                {
                    _logger.LogInformation(
                        "Evento order.cancelled recebido. OrderId: {OrderId}",
                        orderCancelled.Id);

                    try
                    {
                        using var scope = _scopeFactory.CreateScope();

                        var productService = scope.ServiceProvider
                            .GetRequiredService<IProductService>();

                        _logger.LogInformation(
                            "Iniciando cancelamento da reserva de estoque. OrderId: {OrderId}",
                            orderCancelled.Id);

                        var result = await productService
                            .CancelReservationAsync(orderCancelled.Id);

                        if (result)
                        {
                            _logger.LogInformation(
                                "Reserva de estoque cancelada com sucesso. OrderId: {OrderId}",
                                orderCancelled.Id);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Não foi possível cancelar a reserva de estoque. OrderId: {OrderId}",
                                orderCancelled.Id);
                        }

                        var releasedStockEvent = new ReleasedStockEvent
                        {
                            OrderId = orderCancelled.Id
                        };

                        await _publisher.Publish<ReleasedStockEvent>(
                            exchange: "ecommerceEvents",
                            routingKey: "stock.released",
                            message: releasedStockEvent);

                        _logger.LogInformation(
                            "Evento stock.released publicado. OrderId: {OrderId}",
                            orderCancelled.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Erro ao processar evento order.cancelled. OrderId: {OrderId}",
                            orderCancelled.Id);
                    }
                });

            return Task.CompletedTask;
        }
    }
}