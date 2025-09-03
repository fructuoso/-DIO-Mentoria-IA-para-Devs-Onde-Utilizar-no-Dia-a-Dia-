using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using SalesService.Services;
using System.Security.Claims;

namespace SalesService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all orders (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all orders");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Retrieves orders for the current customer
    /// </summary>
    [HttpGet("my-orders")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrders()
    {
        try
        {
            var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(customerId))
                return Unauthorized("Customer ID not found in token");

            var orders = await _orderService.GetOrdersByCustomerAsync(customerId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for current customer");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Retrieves orders for a specific customer (Admin only)
    /// </summary>
    [HttpGet("customer/{customerId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByCustomer(string customerId)
    {
        try
        {
            var orders = await _orderService.GetOrdersByCustomerAsync(customerId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for customer {CustomerId}", customerId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Retrieves a specific order by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            
            if (order == null)
                return NotFound($"Order with ID {id} not found");

            // Check if user can access this order
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            
            if (!isAdmin && order.CustomerId != currentUserId)
                return Forbid("You can only access your own orders");

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Creates a new order
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto createOrderDto)
    {
        try
        {
            // Set customer ID from token
            var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(customerId))
                return Unauthorized("Customer ID not found in token");

            createOrderDto.CustomerId = customerId;

            var order = await _orderService.CreateOrderAsync(createOrderDto);
            
            if (order == null)
                return BadRequest("Unable to create order. Please check product availability.");

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Updates order status (Admin only)
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(status))
                return BadRequest("Status is required");

            var result = await _orderService.UpdateOrderStatusAsync(id, status);
            
            if (!result)
                return NotFound($"Order with ID {id} not found or invalid status");

            return Ok(new { OrderId = id, Status = status });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for order {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Cancels an order
    /// </summary>
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        try
        {
            // Check if user can cancel this order
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound($"Order with ID {id} not found");

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            
            if (!isAdmin && order.CustomerId != currentUserId)
                return Forbid("You can only cancel your own orders");

            var result = await _orderService.CancelOrderAsync(id);
            
            if (!result)
                return BadRequest("Unable to cancel order");

            return Ok(new { OrderId = id, Status = "Cancelled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
