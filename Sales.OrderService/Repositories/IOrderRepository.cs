using Sales.OrderService.Entities;

namespace Sales.OrderService.Repositories
{
    public interface IOrderRepository
    {
        public Task<Order> CreateOrderAsync(Order order);
        public Task<bool> UpdateOrderAsync(Order order);
        public Task<Order?> GetOrderByIdAsync(Guid id);
        public Task<List<Order>> GetOrdersAsync();
    }
}
