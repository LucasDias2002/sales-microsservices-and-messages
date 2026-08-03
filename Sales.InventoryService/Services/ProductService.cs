using AutoMapper;
using Sales.InventoryService.DTOs;
using Sales.InventoryService.Entities;
using Sales.InventoryService.Entities.Enum;
using Sales.InventoryService.Repositories;
using Sales.MessageBus;
using Sales.MessageBus.Messages.InventoryService;
using Sales.MessageBus.Messages.OrderService;

namespace Sales.InventoryService.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IReserveRepository _reserveRepository;
        private readonly IMapper _mapper;

        public ProductService(
            IProductRepository repository,
            IReserveRepository reserveRepository,
            IMapper mapper)
        {
            _repository = repository;
            _reserveRepository = reserveRepository;
            _mapper = mapper;
        }

        public async Task<ProductDTO?> AddProductAsync(ProductDTO productDto)
        {
            var entity = await _repository.AddProductAsync(
                _mapper.Map<Product>(productDto));

            return _mapper.Map<ProductDTO?>(entity);
        }

        public async Task<IEnumerable<ProductDTO>> GetAllAsync()
        {
            var products = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }

        public async Task<ProductDTO?> GetProductByIdAsync(int id)
        {
            var product = await _repository.GetProductByIdAsync(id);
            return _mapper.Map<ProductDTO?>(product);
        }

        public async Task<bool> ReserveStockAsync(OrderCreated order)
        {
            var stockReserved = await _repository.UpdateStockAsync(order);

            if (!stockReserved)
                return false;

            var reserve = new Reserve
            {
                OrderId = order.Id,
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                CreatedAt = DateTime.UtcNow,
                Status = ReservationStatus.Active
            };

            await _reserveRepository.DoReservation(reserve);

            return true;
        }

        public async Task<bool> CancelReservationAsync(Guid orderId)
        {
            var reserve = await _reserveRepository.GetByOrderIdAsync(orderId);

            if (reserve == null)
                return false;

            if (reserve.Status == ReservationStatus.Cancelled)
                return false;

            await _repository.ReturnStockAsync(
                reserve.ProductId,
                reserve.Quantity);

            reserve.Status = ReservationStatus.Cancelled;

            await _reserveRepository.UpdateAsync(reserve);

            return true;
        }

        public async Task<bool> ConfirmReservationAsync(Guid orderId)
        {
            var reserve = await _reserveRepository.GetByOrderIdAsync(orderId);

            if (reserve == null)
                return false;

            reserve.Status = ReservationStatus.Confirmed;

            await _reserveRepository.UpdateAsync(reserve);

            return true;
        }

    }
}
