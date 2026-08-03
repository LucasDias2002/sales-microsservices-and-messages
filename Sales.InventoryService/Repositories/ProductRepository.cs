using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sales.InventoryService.Context;
using Sales.InventoryService.DTOs;
using Sales.InventoryService.Entities;
using Sales.MessageBus.Messages.OrderService;

namespace Sales.InventoryService.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly InventoryContext _context;

        public ProductRepository(InventoryContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product?> AddProductAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> UpdateStockAsync(OrderCreated order)
        {
            var entity = await GetProductByIdAsync(order.ProductId);

            if (entity.Stock == 0 || entity.Stock < order.Quantity)
                return false;

            entity.Stock -= order.Quantity;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReturnStockAsync(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
                return false;

            product.Stock += quantity;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
