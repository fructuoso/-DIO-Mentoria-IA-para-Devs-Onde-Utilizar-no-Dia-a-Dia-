using Microsoft.EntityFrameworkCore;
using Shared.Models;
using SalesService.Data;

namespace SalesService.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly SalesDbContext _context;

    public OrderRepository(SalesDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(string customerId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order> CreateAsync(Order order)
    {
        order.OrderDate = DateTime.UtcNow;
        order.Status = OrderStatus.Pending;
        
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        
        return order;
    }

    public async Task<Order?> UpdateAsync(int id, Order order)
    {
        var existingOrder = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
            
        if (existingOrder == null)
            return null;

        existingOrder.Status = order.Status;
        existingOrder.TotalAmount = order.TotalAmount;
        
        // Update items if needed
        if (order.Items.Any())
        {
            _context.OrderItems.RemoveRange(existingOrder.Items);
            existingOrder.Items = order.Items;
        }

        await _context.SaveChangesAsync();
        return existingOrder;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return false;

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
            return false;

        order.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }
}
