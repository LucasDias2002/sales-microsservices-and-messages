using Sales.MessageBus;
using Sales.MessageBus.Messages.InventoryService;
using Sales.PaymentService.DTOs;

namespace Sales.PaymentService.Services
{
    public class StockReleasedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRabbitMQConsumer _consumer;

        public StockReleasedConsumer(IServiceScopeFactory scopeFactory, IRabbitMQConsumer consumer)
        {
            _scopeFactory = scopeFactory;
            _consumer = consumer;
        }   

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.ConsumeAsync<ReleasedStockEvent>(
                exchange: "ecommerceEvents",
                queue: "payment_stock_released_queue",
                routingKey: "stock.released",
                handler: async (releasedStockEvent) =>
                {
                    var paymentDto = new PaymentProcessed
                    {
                        OrderId = releasedStockEvent.OrderId,
                    };

                    await CancellPayment();
                });
            return Task.CompletedTask;
        }

        //Simulate payment cancellation logic
        private async Task<bool> CancellPayment() {
            return true;
        }
    }
}
