using Sales.InventoryService.DTOs;
using Sales.InventoryService.Services;
using Sales.MessageBus;
using Sales.MessageBus.Messages.InventoryService;
using Sales.MessageBus.Messages.OrderService;
using Sales.MessageBus.Messages.PaymentService;

namespace Sales.InventoryService.Messages
{
    public class OrderCreatedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRabbitMQConsumer _consumer;
        private readonly IRabbitMQPublisher _publisher;
        private readonly ILogger<OrderCreatedConsumer> _logger;

        public OrderCreatedConsumer(
            IServiceScopeFactory scopeFactory,
            IRabbitMQConsumer consumer,
            IRabbitMQPublisher publisher,
            ILogger<OrderCreatedConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _consumer = consumer;
            _publisher = publisher;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "OrderCreatedConsumer iniciado. Aguardando eventos order.created...");

            _consumer.ConsumeAsync<OrderCreated>(
                exchange: "ecommerceEvents",
                queue: "inventory_order_created_queue",
                routingKey: "order.created",
                handler: async (orderCreated) =>
                {
                    _logger.LogInformation(
                        "Evento order.created recebido. OrderId: {OrderId}, ProductId: {ProductId}, Quantity: {Quantity}",
                        orderCreated.Id,
                        orderCreated.ProductId,
                        orderCreated.Quantity);

                    try
                    {
                        using var scope = _scopeFactory.CreateScope();

                        var productService = scope.ServiceProvider
                            .GetRequiredService<IProductService>();

                        _logger.LogInformation(
                            "Buscando produto. ProductId: {ProductId}",
                            orderCreated.ProductId);

                        var product = await productService
                            .GetProductByIdAsync(orderCreated.ProductId);

                        if (product == null)
                        {
                            _logger.LogWarning(
                                "Produto não encontrado. ProductId: {ProductId}, OrderId: {OrderId}",
                                orderCreated.ProductId,
                                orderCreated.Id);

                            return;
                        }

                        _logger.LogInformation(
                            "Produto encontrado. ProductId: {ProductId}, Price: {Price}",
                            orderCreated.ProductId,
                            product.Price);

                        var result = await productService
                            .ReserveStockAsync(orderCreated);

                        if (result)
                        {
                            _logger.LogInformation(
                                "Estoque reservado com sucesso. OrderId: {OrderId}, ProductId: {ProductId}, Quantity: {Quantity}",
                                orderCreated.Id,
                                orderCreated.ProductId,
                                orderCreated.Quantity);

                            var inventoryReservedEvent = new InventoryReservedEvent
                            {
                                OrderId = orderCreated.Id,
                                Success = true,
                                CustomerId = orderCreated.CustomerId,
                                PaymentMethod = orderCreated.PaymentMethod,
                                Amount = orderCreated.Quantity * product.Price
                            };

                            await _publisher.Publish<InventoryReservedEvent>(
                                exchange: "ecommerceEvents",
                                routingKey: "stock.reserved",
                                message: inventoryReservedEvent);

                            _logger.LogInformation(
                                "Evento stock.reserved publicado. OrderId: {OrderId}, Amount: {Amount}",
                                orderCreated.Id,
                                inventoryReservedEvent.Amount);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Não foi possível reservar o estoque. OrderId: {OrderId}, ProductId: {ProductId}, Quantity: {Quantity}",
                                orderCreated.Id,
                                orderCreated.ProductId,
                                orderCreated.Quantity);

                            var notReserved = new InventoryNotReservedEvent
                            {
                                OrderId = orderCreated.Id,
                                Success = false,
                                Message = "Insufficient stock"
                            };

                            await _publisher.Publish<InventoryNotReservedEvent>(
                                exchange: "ecommerceEvents",
                                routingKey: "stock.not.reserved",
                                message: notReserved);

                            _logger.LogInformation(
                                "Evento stock.not.reserved publicado. OrderId: {OrderId}",
                                orderCreated.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Erro ao processar evento order.created. OrderId: {OrderId}",
                            orderCreated.Id);
                    }
                });

            return Task.CompletedTask;
        }
    }
}