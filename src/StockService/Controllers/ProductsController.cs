using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using StockService.Services;

namespace StockService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all products
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Retrieves a specific product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            
            if (product == null)
                return NotFound($"Product with ID {id} not found");

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createProductDto)
    {
        try
        {
            var product = await _productService.CreateProductAsync(createProductDto);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, UpdateProductDto updateProductDto)
    {
        try
        {
            var product = await _productService.UpdateProductAsync(id, updateProductDto);
            
            if (product == null)
                return NotFound($"Product with ID {id} not found");

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Deletes a product
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            var result = await _productService.DeleteProductAsync(id);
            
            if (!result)
                return NotFound($"Product with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Checks stock availability for a product
    /// </summary>
    [HttpGet("{id}/stock/check/{quantity}")]
    public async Task<ActionResult<bool>> CheckStockAvailability(int id, int quantity)
    {
        try
        {
            if (quantity <= 0)
                return BadRequest("Quantity must be greater than zero");

            var isAvailable = await _productService.CheckStockAvailabilityAsync(id, quantity);
            return Ok(new { ProductId = id, RequestedQuantity = quantity, IsAvailable = isAvailable });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking stock availability for product {ProductId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Updates product stock
    /// </summary>
    [HttpPut("{id}/stock")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] int newQuantity)
    {
        try
        {
            if (newQuantity < 0)
                return BadRequest("Stock quantity cannot be negative");

            var result = await _productService.UpdateStockAsync(id, newQuantity);
            
            if (!result)
                return NotFound($"Product with ID {id} not found");

            return Ok(new { ProductId = id, NewQuantity = newQuantity });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock for product {ProductId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Reserves stock for a product
    /// </summary>
    [HttpPost("{id}/stock/reserve")]
    public async Task<IActionResult> ReserveStock(int id, [FromBody] int quantity)
    {
        try
        {
            if (quantity <= 0)
                return BadRequest("Quantity must be greater than zero");

            var result = await _productService.ReserveStockAsync(id, quantity);
            
            if (!result)
                return BadRequest($"Unable to reserve {quantity} units of product {id}. Insufficient stock or product not found.");

            return Ok(new { ProductId = id, ReservedQuantity = quantity });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving stock for product {ProductId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
