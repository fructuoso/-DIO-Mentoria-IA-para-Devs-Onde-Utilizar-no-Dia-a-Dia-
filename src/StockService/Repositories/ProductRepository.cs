using Microsoft.EntityFrameworkCore;
using Shared.Models;
using StockService.Data;

namespace StockService.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly StockDbContext _context;

    public ProductRepository(StockDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> UpdateAsync(int id, Product product)
    {
        var existingProduct = await _context.Products.FindAsync(id);
        if (existingProduct == null)
            return null;

        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.Price = product.Price;
        existingProduct.StockQuantity = product.StockQuantity;
        existingProduct.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingProduct;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStockAsync(int productId, int newQuantity)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return false;

        product.StockQuantity = newQuantity;
        product.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DecrementStockAsync(int productId, int quantity)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null || product.StockQuantity < quantity)
            return false;

        product.StockQuantity -= quantity;
        product.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IncrementStockAsync(int productId, int quantity)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return false;

        product.StockQuantity += quantity;
        product.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}
