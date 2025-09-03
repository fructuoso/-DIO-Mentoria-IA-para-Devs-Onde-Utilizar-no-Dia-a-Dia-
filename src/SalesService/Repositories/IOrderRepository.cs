using Shared.Models;

namespace SalesService.Repositories;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllAsync();
    Task<IEnumerable<Order>> GetByCustomerIdAsync(string customerId);
    Task<Order?> GetByIdAsync(int id);
    Task<Order> CreateAsync(Order order);
    Task<Order?> UpdateAsync(int id, Order order);
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateStatusAsync(int orderId, OrderStatus status);
}
