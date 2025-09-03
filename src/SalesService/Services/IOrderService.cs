using Shared.DTOs;

namespace SalesService.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<IEnumerable<OrderDto>> GetOrdersByCustomerAsync(string customerId);
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<OrderDto?> CreateOrderAsync(CreateOrderDto createOrderDto);
    Task<bool> UpdateOrderStatusAsync(int orderId, string status);
    Task<bool> CancelOrderAsync(int orderId);
}
