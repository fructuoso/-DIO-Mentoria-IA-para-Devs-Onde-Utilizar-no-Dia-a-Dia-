using AutoMapper;
using Shared.DTOs;
using Shared.Messaging;
using Shared.Models;
using SalesService.Repositories;

namespace SalesService.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IStockService _stockService;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IStockService stockService,
        IMessagePublisher messagePublisher,
        IMapper mapper,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _stockService = stockService;
        _messagePublisher = messagePublisher;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        try
        {
            var orders = await _orderRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all orders");
            throw;
        }
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerAsync(string customerId)
    {
        try
        {
            var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for customer {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order != null ? _mapper.Map<OrderDto>(order) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", id);
            throw;
        }
    }

    public async Task<OrderDto?> CreateOrderAsync(CreateOrderDto createOrderDto)
    {
        try
        {
            _logger.LogInformation("Creating order for customer {CustomerId} with {ItemCount} items", 
                createOrderDto.CustomerId, createOrderDto.Items.Count);

            // Validate stock availability for all items
            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            foreach (var itemDto in createOrderDto.Items)
            {
                // Get product information
                var product = await _stockService.GetProductAsync(itemDto.ProductId);
                if (product == null)
                {
                    _logger.LogWarning("Product {ProductId} not found", itemDto.ProductId);
                    return null;
                }

                // Check stock availability
                var isAvailable = await _stockService.CheckStockAvailabilityAsync(itemDto.ProductId, itemDto.Quantity);
                if (!isAvailable)
                {
                    _logger.LogWarning("Insufficient stock for product {ProductId}. Requested: {Quantity}", 
                        itemDto.ProductId, itemDto.Quantity);
                    return null;
                }

                var orderItem = new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    ProductName = product.Name,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price * itemDto.Quantity
                };

                orderItems.Add(orderItem);
                totalAmount += orderItem.TotalPrice;
            }

            // Create the order
            var order = new Order
            {
                CustomerId = createOrderDto.CustomerId,
                Items = orderItems,
                TotalAmount = totalAmount
            };

            var createdOrder = await _orderRepository.CreateAsync(order);

            // Reserve stock for all items
            bool allReservationsSuccessful = true;
            foreach (var item in orderItems)
            {
                var reservationSuccess = await _stockService.ReserveStockAsync(item.ProductId, item.Quantity);
                if (!reservationSuccess)
                {
                    _logger.LogError("Failed to reserve stock for product {ProductId} in order {OrderId}", 
                        item.ProductId, createdOrder.Id);
                    allReservationsSuccessful = false;
                    break;
                }

                // Send stock update message
                var stockUpdate = new StockUpdateDto
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };
                await _messagePublisher.PublishAsync("stock-updates", stockUpdate);
            }

            if (!allReservationsSuccessful)
            {
                // Cancel the order if stock reservation failed
                await _orderRepository.UpdateStatusAsync(createdOrder.Id, OrderStatus.Cancelled);
                _logger.LogWarning("Order {OrderId} was cancelled due to stock reservation failure", createdOrder.Id);
                return null;
            }

            // Confirm the order
            await _orderRepository.UpdateStatusAsync(createdOrder.Id, OrderStatus.Confirmed);

            _logger.LogInformation("Order {OrderId} created successfully for customer {CustomerId}", 
                createdOrder.Id, createOrderDto.CustomerId);

            return _mapper.Map<OrderDto>(createdOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for customer {CustomerId}", createOrderDto.CustomerId);
            throw;
        }
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
    {
        try
        {
            if (!Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            {
                _logger.LogWarning("Invalid order status: {Status}", status);
                return false;
            }

            var result = await _orderRepository.UpdateStatusAsync(orderId, orderStatus);
            
            if (result)
                _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, status);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for order {OrderId}", orderId);
            throw;
        }
    }

    public async Task<bool> CancelOrderAsync(int orderId)
    {
        try
        {
            var result = await _orderRepository.UpdateStatusAsync(orderId, OrderStatus.Cancelled);
            
            if (result)
                _logger.LogInformation("Order {OrderId} cancelled", orderId);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
            throw;
        }
    }
}
