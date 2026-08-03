using Sales.InventoryService.DTOs;
using Sales.InventoryService.Entities;
using Sales.MessageBus.Messages.OrderService;

namespace Sales.InventoryService.Repositories
{
    public interface IProductRepository
    {
        public Task<Product?> GetProductByIdAsync(int id);
        public Task<IEnumerable<Product>> GetAllAsync();
        public Task<bool> UpdateStockAsync(OrderCreated order);
        public Task<Product?> AddProductAsync(Product product);
        public Task<bool> ReturnStockAsync(int productId, int quantity);
    }
}
