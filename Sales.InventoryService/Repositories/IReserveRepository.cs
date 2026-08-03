using Sales.InventoryService.Entities;

namespace Sales.InventoryService.Repositories
{
    public interface IReserveRepository
    {
        Task<Reserve> DoReservation(Reserve reserve);

        Task<Reserve?> GetByOrderIdAsync(Guid orderId);

        Task<bool> UpdateAsync(Reserve reserve);

        Task<bool> CancelReserveAsync(Guid orderId);
    }
}