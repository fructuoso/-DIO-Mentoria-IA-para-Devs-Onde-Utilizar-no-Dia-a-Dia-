using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.DTOs;
using Shared.Models;
using StockService.Mappings;
using StockService.Repositories;
using StockService.Services;
using Xunit;

namespace StockService.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly IMapper _mapper;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockLogger = new Mock<ILogger<ProductService>>();
        
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProductMappingProfile>();
        });
        _mapper = configuration.CreateMapper();
        
        _service = new ProductService(_mockRepository.Object, _mapper, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllProductsAsync_ShouldReturnMappedProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Description = "Desc 1", Price = 10.00m, StockQuantity = 5 },
            new Product { Id = 2, Name = "Product 2", Description = "Desc 2", Price = 20.00m, StockQuantity = 10 }
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _service.GetAllProductsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Equal("Product 1", result.First().Name);
        
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetProductByIdAsync_WithValidId_ShouldReturnMappedProduct()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Product", Description = "Test Desc", Price = 15.00m, StockQuantity = 8 };
        
        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        // Act
        var result = await _service.GetProductByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal(15.00m, result.Price);
        
        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetProductByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _service.GetProductByIdAsync(999);

        // Assert
        Assert.Null(result);
        
        _mockRepository.Verify(r => r.GetByIdAsync(999), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldCreateAndReturnMappedProduct()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "New Product",
            Description = "New Description",
            Price = 25.00m,
            StockQuantity = 12
        };

        var createdProduct = new Product
        {
            Id = 1,
            Name = createDto.Name,
            Description = createDto.Description,
            Price = createDto.Price,
            StockQuantity = createDto.StockQuantity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Product>()))
            .ReturnsAsync(createdProduct);

        // Act
        var result = await _service.CreateProductAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Product", result.Name);
        Assert.Equal(25.00m, result.Price);
        
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task CheckStockAvailabilityAsync_WithSufficientStock_ShouldReturnTrue()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Product", StockQuantity = 10 };
        
        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        // Act
        var result = await _service.CheckStockAvailabilityAsync(1, 5);

        // Assert
        Assert.True(result);
        
        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task CheckStockAvailabilityAsync_WithInsufficientStock_ShouldReturnFalse()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Product", StockQuantity = 3 };
        
        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        // Act
        var result = await _service.CheckStockAvailabilityAsync(1, 5);

        // Assert
        Assert.False(result);
        
        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task ReserveStockAsync_WithSufficientStock_ShouldReturnTrue()
    {
        // Arrange
        _mockRepository.Setup(r => r.DecrementStockAsync(1, 5))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ReserveStockAsync(1, 5);

        // Assert
        Assert.True(result);
        
        _mockRepository.Verify(r => r.DecrementStockAsync(1, 5), Times.Once);
    }

    [Fact]
    public async Task ReserveStockAsync_WithInsufficientStock_ShouldReturnFalse()
    {
        // Arrange
        _mockRepository.Setup(r => r.DecrementStockAsync(1, 10))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ReserveStockAsync(1, 10);

        // Assert
        Assert.False(result);
        
        _mockRepository.Verify(r => r.DecrementStockAsync(1, 10), Times.Once);
    }
}
