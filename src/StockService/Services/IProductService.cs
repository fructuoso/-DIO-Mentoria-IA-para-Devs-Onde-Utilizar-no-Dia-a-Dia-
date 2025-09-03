using Shared.DTOs;

namespace StockService.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
    Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
    Task<bool> DeleteProductAsync(int id);
    Task<bool> UpdateStockAsync(int productId, int newQuantity);
    Task<bool> CheckStockAvailabilityAsync(int productId, int quantity);
    Task<bool> ReserveStockAsync(int productId, int quantity);
}
