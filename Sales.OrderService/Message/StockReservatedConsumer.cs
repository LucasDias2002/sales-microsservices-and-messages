using Sales.MessageBus;
using Sales.MessageBus.Messages.InventoryService;
using Sales.OrderService.DTOs;
using Sales.OrderService.Entities.Enums;
using Sales.OrderService.Services;

namespace Sales.OrderService.Message
{
    public class StockReservedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRabbitMQConsumer _consumer;
        private readonly ILogger<StockReservedConsumer> _logger;

        public StockReservedConsumer(
            IServiceScopeFactory scopeFactory,
            IRabbitMQConsumer consumer,
            ILogger<StockReservedConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _consumer = consumer;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "StockReservedConsumer iniciado. Aguardando eventos stock.reserved...");

            _consumer.ConsumeAsync<InventoryReservedEvent>(
                exchange: "ecommerceEvents",
                queue: "order_stock_reserved_queue",
                routingKey: "stock.reserved",
                handler: async (reservationEvent) =>
                {
                    _logger.LogInformation(
                        "Evento stock.reserved recebido. OrderId: {OrderId}, Amount: {Amount}",
                        reservationEvent.OrderId,
                        reservationEvent.Amount);

                    try
                    {
                        using var scope = _scopeFactory.CreateScope();

                        var orderService = scope.ServiceProvider
                            .GetRequiredService<IOrderService>();

                        var order = new OrderDTO
                        {
                            Id = reservationEvent.OrderId,
                            Status = OrderStatus.AwaitingPayment
                        };

                        _logger.LogInformation(
                            "Atualizando status do pedido para AwaitingPayment. OrderId: {OrderId}",
                            order.Id);

                        var result = await orderService.UpdateStatusAsync(
                            order,
                            order.Status);

                        if (result)
                        {
                            _logger.LogInformation(
                                "Pedido atualizado para AwaitingPayment com sucesso. OrderId: {OrderId}",
                                order.Id);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Não foi possível atualizar o pedido para AwaitingPayment. OrderId: {OrderId}",
                                order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Erro ao processar stock.reserved. OrderId: {OrderId}",
                            reservationEvent.OrderId);
                    }
                });

            return Task.CompletedTask;
        }
    }
}