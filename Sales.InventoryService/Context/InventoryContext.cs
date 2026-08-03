using Microsoft.EntityFrameworkCore;
using Sales.InventoryService.Entities;

namespace Sales.InventoryService.Context
{
    public class InventoryContext: DbContext
    {
        public InventoryContext(DbContextOptions<InventoryContext> options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Reserve> Reservations { get; set; }
    }
}