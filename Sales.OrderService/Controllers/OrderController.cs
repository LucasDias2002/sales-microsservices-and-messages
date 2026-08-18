using Microsoft.AspNetCore.Mvc;
using Sales.OrderService.DTOs;
using Sales.OrderService.Services;

namespace Sales.OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDTO order)
        {
            var createdOrder = await _orderService.CreateOrderAsync(order);
            return Ok(createdOrder);
        }

        [HttpGet]
        public async Task<IActionResult> GetListOrders()
        {
            var createdOrder = await _orderService.GetOrdersAsync();
            return Ok(createdOrder);
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var createdOrder = await _orderService.GetOrderByIdAsync(id);
            if (createdOrder == null)
                return NotFound();
            return Ok(createdOrder);
        }
    }
}
