using AutoMapper;
using Microsoft.Extensions.Logging;
using Shared.DTOs;
using Shared.Models;
using StockService.Repositories;

namespace StockService.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository productRepository, IMapper mapper, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        try
        {
            var products = await _productRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all products");
            throw;
        }
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product with ID {ProductId}", id);
            throw;
        }
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
    {
        try
        {
            var product = _mapper.Map<Product>(createProductDto);
            var createdProduct = await _productRepository.CreateAsync(product);
            
            _logger.LogInformation("Product created with ID {ProductId}", createdProduct.Id);
            return _mapper.Map<ProductDto>(createdProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            throw;
        }
    }

    public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
    {
        try
        {
            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
                return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateProductDto.Name))
                existingProduct.Name = updateProductDto.Name;
            
            if (!string.IsNullOrEmpty(updateProductDto.Description))
                existingProduct.Description = updateProductDto.Description;
            
            if (updateProductDto.Price.HasValue)
                existingProduct.Price = updateProductDto.Price.Value;
            
            if (updateProductDto.StockQuantity.HasValue)
                existingProduct.StockQuantity = updateProductDto.StockQuantity.Value;

            var updatedProduct = await _productRepository.UpdateAsync(id, existingProduct);
            
            if (updatedProduct != null)
                _logger.LogInformation("Product updated with ID {ProductId}", id);
            
            return updatedProduct != null ? _mapper.Map<ProductDto>(updatedProduct) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID {ProductId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        try
        {
            var result = await _productRepository.DeleteAsync(id);
            
            if (result)
                _logger.LogInformation("Product deleted with ID {ProductId}", id);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID {ProductId}", id);
            throw;
        }
    }

    public async Task<bool> UpdateStockAsync(int productId, int newQuantity)
    {
        try
        {
            var result = await _productRepository.UpdateStockAsync(productId, newQuantity);
            
            if (result)
                _logger.LogInformation("Stock updated for product {ProductId} to {NewQuantity}", productId, newQuantity);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock for product {ProductId}", productId);
            throw;
        }
    }

    public async Task<bool> CheckStockAvailabilityAsync(int productId, int quantity)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(productId);
            return product != null && product.StockQuantity >= quantity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking stock availability for product {ProductId}", productId);
            throw;
        }
    }

    public async Task<bool> ReserveStockAsync(int productId, int quantity)
    {
        try
        {
            var result = await _productRepository.DecrementStockAsync(productId, quantity);
            
            if (result)
                _logger.LogInformation("Stock reserved for product {ProductId}: {Quantity} units", productId, quantity);
            else
                _logger.LogWarning("Failed to reserve stock for product {ProductId}: {Quantity} units", productId, quantity);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving stock for product {ProductId}", productId);
            throw;
        }
    }
}
