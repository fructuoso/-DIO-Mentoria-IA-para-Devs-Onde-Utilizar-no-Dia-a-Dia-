using Shared.DTOs;

namespace SalesService.Services;

public interface IStockService
{
    Task<ProductDto?> GetProductAsync(int productId);
    Task<bool> CheckStockAvailabilityAsync(int productId, int quantity);
    Task<bool> ReserveStockAsync(int productId, int quantity);
}
