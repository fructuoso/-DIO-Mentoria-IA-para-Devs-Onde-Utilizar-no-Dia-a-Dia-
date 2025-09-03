using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SalesService.Mappings;
using SalesService.Repositories;
using SalesService.Services;
using Shared.DTOs;
using Shared.Messaging;
using Shared.Models;
using Xunit;

namespace SalesService.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<IStockService> _mockStockService;
    private readonly Mock<IMessagePublisher> _mockMessagePublisher;
    private readonly Mock<ILogger<OrderService>> _mockLogger;
    private readonly IMapper _mapper;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockStockService = new Mock<IStockService>();
        _mockMessagePublisher = new Mock<IMessagePublisher>();
        _mockLogger = new Mock<ILogger<OrderService>>();
        
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<OrderMappingProfile>();
        });
        _mapper = configuration.CreateMapper();
        
        _service = new OrderService(
            _mockOrderRepository.Object,
            _mockStockService.Object,
            _mockMessagePublisher.Object,
            _mapper,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllOrdersAsync_ShouldReturnMappedOrders()
    {
        // Arrange
        var orders = new List<Order>
        {
            new Order 
            { 
                Id = 1, 
                CustomerId = "customer1", 
                Status = OrderStatus.Confirmed,
                TotalAmount = 100.00m,
                Items = new List<OrderItem>()
            },
            new Order 
            { 
                Id = 2, 
                CustomerId = "customer2", 
                Status = OrderStatus.Pending,
                TotalAmount = 200.00m,
                Items = new List<OrderItem>()
            }
        };

        _mockOrderRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(orders);

        // Act
        var result = await _service.GetAllOrdersAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Equal("Confirmed", result.First().Status);
        
        _mockOrderRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetOrdersByCustomerAsync_ShouldReturnCustomerOrders()
    {
        // Arrange
        var customerId = "customer1";
        var orders = new List<Order>
        {
            new Order 
            { 
                Id = 1, 
                CustomerId = customerId, 
                Status = OrderStatus.Confirmed,
                TotalAmount = 100.00m,
                Items = new List<OrderItem>()
            }
        };

        _mockOrderRepository.Setup(r => r.GetByCustomerIdAsync(customerId))
            .ReturnsAsync(orders);

        // Act
        var result = await _service.GetOrdersByCustomerAsync(customerId);

        // Assert
        Assert.Single(result);
        Assert.Equal(customerId, result.First().CustomerId);
        
        _mockOrderRepository.Verify(r => r.GetByCustomerIdAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidItems_ShouldCreateOrder()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = "customer1",
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = 1, Quantity = 2 }
            }
        };

        var product = new ProductDto
        {
            Id = 1,
            Name = "Test Product",
            Price = 50.00m,
            StockQuantity = 10
        };

        var createdOrder = new Order
        {
            Id = 1,
            CustomerId = "customer1",
            Status = OrderStatus.Confirmed,
            TotalAmount = 100.00m,
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductId = 1,
                    ProductName = "Test Product",
                    Quantity = 2,
                    UnitPrice = 50.00m,
                    TotalPrice = 100.00m
                }
            }
        };

        _mockStockService.Setup(s => s.GetProductAsync(1))
            .ReturnsAsync(product);
        
        _mockStockService.Setup(s => s.CheckStockAvailabilityAsync(1, 2))
            .ReturnsAsync(true);
            
        _mockStockService.Setup(s => s.ReserveStockAsync(1, 2))
            .ReturnsAsync(true);

        _mockOrderRepository.Setup(r => r.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync(createdOrder);
            
        _mockOrderRepository.Setup(r => r.UpdateStatusAsync(It.IsAny<int>(), OrderStatus.Confirmed))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateOrderAsync(createOrderDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("customer1", result.CustomerId);
        Assert.Equal(100.00m, result.TotalAmount);
        
        _mockStockService.Verify(s => s.GetProductAsync(1), Times.Once);
        _mockStockService.Verify(s => s.CheckStockAvailabilityAsync(1, 2), Times.Once);
        _mockStockService.Verify(s => s.ReserveStockAsync(1, 2), Times.Once);
        _mockOrderRepository.Verify(r => r.CreateAsync(It.IsAny<Order>()), Times.Once);
        _mockMessagePublisher.Verify(m => m.PublishAsync("stock-updates", It.IsAny<StockUpdateDto>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_WithInvalidProduct_ShouldReturnNull()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = "customer1",
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = 999, Quantity = 2 }
            }
        };

        _mockStockService.Setup(s => s.GetProductAsync(999))
            .ReturnsAsync((ProductDto?)null);

        // Act
        var result = await _service.CreateOrderAsync(createOrderDto);

        // Assert
        Assert.Null(result);
        
        _mockStockService.Verify(s => s.GetProductAsync(999), Times.Once);
        _mockOrderRepository.Verify(r => r.CreateAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_WithInsufficientStock_ShouldReturnNull()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = "customer1",
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = 1, Quantity = 10 }
            }
        };

        var product = new ProductDto
        {
            Id = 1,
            Name = "Test Product",
            Price = 50.00m,
            StockQuantity = 5
        };

        _mockStockService.Setup(s => s.GetProductAsync(1))
            .ReturnsAsync(product);
        
        _mockStockService.Setup(s => s.CheckStockAvailabilityAsync(1, 10))
            .ReturnsAsync(false);

        // Act
        var result = await _service.CreateOrderAsync(createOrderDto);

        // Assert
        Assert.Null(result);
        
        _mockStockService.Verify(s => s.GetProductAsync(1), Times.Once);
        _mockStockService.Verify(s => s.CheckStockAvailabilityAsync(1, 10), Times.Once);
        _mockOrderRepository.Verify(r => r.CreateAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithValidStatus_ShouldReturnTrue()
    {
        // Arrange
        var orderId = 1;
        var status = "Shipped";

        _mockOrderRepository.Setup(r => r.UpdateStatusAsync(orderId, OrderStatus.Shipped))
            .ReturnsAsync(true);

        // Act
        var result = await _service.UpdateOrderStatusAsync(orderId, status);

        // Assert
        Assert.True(result);
        
        _mockOrderRepository.Verify(r => r.UpdateStatusAsync(orderId, OrderStatus.Shipped), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithInvalidStatus_ShouldReturnFalse()
    {
        // Arrange
        var orderId = 1;
        var status = "InvalidStatus";

        // Act
        var result = await _service.UpdateOrderStatusAsync(orderId, status);

        // Assert
        Assert.False(result);
        
        _mockOrderRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<int>(), It.IsAny<OrderStatus>()), Times.Never);
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldUpdateStatusToCancelled()
    {
        // Arrange
        var orderId = 1;

        _mockOrderRepository.Setup(r => r.UpdateStatusAsync(orderId, OrderStatus.Cancelled))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CancelOrderAsync(orderId);

        // Assert
        Assert.True(result);
        
        _mockOrderRepository.Verify(r => r.UpdateStatusAsync(orderId, OrderStatus.Cancelled), Times.Once);
    }
}
