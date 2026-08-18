using Sales.OrderService.DTOs;
using Sales.OrderService.Entities.Enums;

namespace Sales.OrderService.Services
{
    public interface IOrderService
    {
        public Task<OrderDTO> CreateOrderAsync(OrderDTO order);
        public Task<bool> UpdateStatusAsync(OrderDTO order, OrderStatus status);
        public Task<List<OrderDTO>> GetOrdersAsync();
        public Task<OrderDTO?> GetOrderByIdAsync(Guid id);
    }
}
