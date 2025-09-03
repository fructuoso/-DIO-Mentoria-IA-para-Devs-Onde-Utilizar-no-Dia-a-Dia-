using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.DTOs;
using Shared.Models;
using StockService.Data;
using StockService.Mappings;
using StockService.Repositories;
using StockService.Services;
using Xunit;

namespace StockService.Tests.Repositories;

public class ProductRepositoryTests : IDisposable
{
    private readonly StockDbContext _context;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<StockDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StockDbContext(options);
        _repository = new ProductRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Price = 10.00m, StockQuantity = 5 },
            new Product { Id = 2, Name = "Product 2", Price = 20.00m, StockQuantity = 10 }
        };

        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Product", Price = 15.00m, StockQuantity = 8 };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Product", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddProduct()
    {
        // Arrange
        var product = new Product { Name = "New Product", Price = 25.00m, StockQuantity = 12 };

        // Act
        var result = await _repository.CreateAsync(product);

        // Assert
        Assert.True(result.Id > 0);
        Assert.Equal("New Product", result.Name);
        
        var productInDb = await _context.Products.FindAsync(result.Id);
        Assert.NotNull(productInDb);
    }

    [Fact]
    public async Task DecrementStockAsync_WithSufficientStock_ShouldReturnTrue()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Product", Price = 15.00m, StockQuantity = 10 };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DecrementStockAsync(1, 5);

        // Assert
        Assert.True(result);
        
        var updatedProduct = await _context.Products.FindAsync(1);
        Assert.Equal(5, updatedProduct?.StockQuantity);
    }

    [Fact]
    public async Task DecrementStockAsync_WithInsufficientStock_ShouldReturnFalse()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Product", Price = 15.00m, StockQuantity = 3 };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DecrementStockAsync(1, 5);

        // Assert
        Assert.False(result);
        
        var updatedProduct = await _context.Products.FindAsync(1);
        Assert.Equal(3, updatedProduct?.StockQuantity); // Should remain unchanged
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
