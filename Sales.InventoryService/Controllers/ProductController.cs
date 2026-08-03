using Microsoft.AspNetCore.Mvc;
using Sales.InventoryService.DTOs;
using Sales.InventoryService.Services;

namespace Sales.InventoryService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost("add-product")]
        public async Task<IActionResult> AddProduct([FromBody] ProductDTO productDto)
        {
            // Implementation for adding a new product
            var result = await _productService.AddProductAsync(productDto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            // Implementation for retrieving all products
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }
    }
}
