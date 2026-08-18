using AutoMapper;
using Sales.MessageBus;
using Sales.MessageBus.Messages.OrderService;
using Sales.OrderService.DTOs;
using Sales.OrderService.Entities;
using Sales.OrderService.Entities.Enums;
using Sales.OrderService.Repositories;

namespace Sales.OrderService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IRabbitMQPublisher _rabbitMQPublisher;
        public OrderService(IOrderRepository orderRepository, IMapper mapper, IRabbitMQPublisher rabbitMQPublisher)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _rabbitMQPublisher = rabbitMQPublisher;
        }
        public async Task<OrderDTO> CreateOrderAsync(OrderDTO order)
        {
            var orderEntity = _mapper.Map<Order>(order);

            orderEntity.CreatedAt = orderEntity.UpdatedAt = DateTime.UtcNow;
            orderEntity.Status = OrderStatus.Pending;

            var createdOrder = await _orderRepository.CreateOrderAsync(orderEntity);

            await _rabbitMQPublisher.Publish<OrderCreated>(new OrderCreated
            {
                Id = createdOrder.Id,
                CustomerId = createdOrder.CustomerId,
                ProductId = order.ProductId,
                PaymentMethod = createdOrder.PaymentMethod.ToString(),
                CreatedAt = createdOrder.CreatedAt,
                Quantity = createdOrder.Quantity,
            }, "ecommerceEvents", "order.created");

            return _mapper.Map<OrderDTO>(createdOrder);
        }

        public async Task<OrderDTO?> GetOrderByIdAsync(Guid id)
        {
            var orderEntity = await _orderRepository.GetOrderByIdAsync(id);
            return _mapper.Map<OrderDTO>(orderEntity);
        }

        public async Task<List<OrderDTO>> GetOrdersAsync()
        {
            var orders = await _orderRepository.GetOrdersAsync();
            return _mapper.Map<List<OrderDTO>>(orders);
        }

        public async Task<bool> UpdateStatusAsync(OrderDTO order, OrderStatus status)
        {
            var orderEntity = await _orderRepository.GetOrderByIdAsync(order.Id);
            if (orderEntity == null)
                return false;

            orderEntity.Status = status;
            orderEntity.UpdatedAt = DateTime.UtcNow;
            orderEntity.CancellationReason = order.CancellationReason;

            await _orderRepository.UpdateOrderAsync(orderEntity);
            return true;
        }
    }
}
