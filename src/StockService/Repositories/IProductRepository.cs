using Shared.Models;

namespace StockService.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(Product product);
    Task<Product?> UpdateAsync(int id, Product product);
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateStockAsync(int productId, int newQuantity);
    Task<bool> DecrementStockAsync(int productId, int quantity);
    Task<bool> IncrementStockAsync(int productId, int quantity);
}
