using Sales.MessageBus;
using Sales.MessageBus.Messages.InventoryService;
using Sales.OrderService.DTOs;
using Sales.OrderService.Entities.Enums;
using Sales.OrderService.Services;

namespace Sales.OrderService.Message
{
    public class StockNotReservedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRabbitMQConsumer _consumer;
        private readonly ILogger<StockNotReservedConsumer> _logger;

        public StockNotReservedConsumer(
            IServiceScopeFactory scopeFactory,
            IRabbitMQConsumer consumer,
            ILogger<StockNotReservedConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _consumer = consumer;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "StockNotReservedConsumer iniciado. Aguardando eventos stock.not.reserved...");

            _consumer.ConsumeAsync<InventoryNotReservedEvent>(
                exchange: "ecommerceEvents",
                queue: "order_stock_not_reservation_queue",
                routingKey: "stock.not.reserved",
                handler: async (notReservedEvent) =>
                {
                    _logger.LogWarning(
                        "Evento stock.not.reserved recebido. OrderId: {OrderId}, Motivo: {Reason}",
                        notReservedEvent.OrderId,
                        notReservedEvent.Message);

                    try
                    {
                        using var scope = _scopeFactory.CreateScope();

                        var orderService = scope.ServiceProvider
                            .GetRequiredService<IOrderService>();

                        var order = new OrderDTO
                        {
                            Id = notReservedEvent.OrderId,
                            Status = OrderStatus.Cancelled,
                            CancellationReason = notReservedEvent.Message
                        };

                        _logger.LogWarning(
                            "Cancelando pedido devido à falha na reserva de estoque. " +
                            "OrderId: {OrderId}, Motivo: {Reason}",
                            order.Id,
                            order.CancellationReason);

                        var result = await orderService.UpdateStatusAsync(
                            order,
                            order.Status);

                        if (result)
                        {
                            _logger.LogInformation(
                                "Pedido cancelado com sucesso. OrderId: {OrderId}",
                                order.Id);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Não foi possível atualizar o status do pedido. OrderId: {OrderId}",
                                order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Erro ao processar stock.not.reserved. OrderId: {OrderId}",
                            notReservedEvent.OrderId);
                    }
                });

            return Task.CompletedTask;
        }
    }
}