using Microsoft.EntityFrameworkCore;
using Sales.InventoryService.Context;
using Sales.InventoryService.Entities;
using Sales.InventoryService.Entities.Enum;

namespace Sales.InventoryService.Repositories
{
    public class ReserveRepository : IReserveRepository
    {
        private readonly InventoryContext _context;

        public ReserveRepository(InventoryContext context)
        {
            _context = context;
        }

        public async Task<Reserve> DoReservation(Reserve reserve)
        {
            var result = await _context.Reservations.AddAsync(reserve);

            await _context.SaveChangesAsync();

            return result.Entity;
        }

        public async Task<Reserve?> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.Reservations
                .FirstOrDefaultAsync(r => r.OrderId == orderId);
        }

        public async Task<bool> CancelReserveAsync(Guid orderId)
        {
            var reserve = await GetByOrderIdAsync(orderId);

            if (reserve == null)
                return false;

            reserve.Status = ReservationStatus.Cancelled;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateAsync(Reserve reserve)
        {
            _context.Reservations.Update(reserve);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}