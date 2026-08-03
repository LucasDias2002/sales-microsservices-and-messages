using Sales.InventoryService.DTOs;
using Sales.MessageBus.Messages.OrderService;

namespace Sales.InventoryService.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetAllAsync();

        Task<ProductDTO?> GetProductByIdAsync(int id);

        Task<ProductDTO?> AddProductAsync(ProductDTO productDto);

        Task<bool> ReserveStockAsync(OrderCreated order);

        Task<bool> CancelReservationAsync(Guid orderId);

        Task<bool> ConfirmReservationAsync(Guid orderId);
    }
}