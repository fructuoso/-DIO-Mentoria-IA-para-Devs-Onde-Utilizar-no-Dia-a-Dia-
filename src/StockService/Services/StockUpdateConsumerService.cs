using Shared.DTOs;
using Shared.Messaging;
using StockService.Services;

namespace StockService.Services;

public class StockUpdateConsumerService : IHostedService
{
    private readonly IMessageConsumer _messageConsumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StockUpdateConsumerService> _logger;

    public StockUpdateConsumerService(
        IMessageConsumer messageConsumer,
        IServiceProvider serviceProvider,
        ILogger<StockUpdateConsumerService> logger)
    {
        _messageConsumer = messageConsumer;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Stock Update Consumer Service");
        
        await _messageConsumer.StartConsumingAsync<StockUpdateDto>("stock-updates", HandleStockUpdate);
        
        _logger.LogInformation("Stock Update Consumer Service started");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Stock Update Consumer Service");
        
        _messageConsumer.StopConsuming();
        
        _logger.LogInformation("Stock Update Consumer Service stopped");
        return Task.CompletedTask;
    }

    private async Task HandleStockUpdate(StockUpdateDto stockUpdate)
    {
        using var scope = _serviceProvider.CreateScope();
        var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

        try
        {
            _logger.LogInformation("Processing stock update for product {ProductId}: {Quantity}", 
                stockUpdate.ProductId, stockUpdate.Quantity);

            var success = await productService.ReserveStockAsync(stockUpdate.ProductId, stockUpdate.Quantity);
            
            if (success)
            {
                _logger.LogInformation("Stock update processed successfully for product {ProductId}", 
                    stockUpdate.ProductId);
            }
            else
            {
                _logger.LogWarning("Failed to process stock update for product {ProductId}", 
                    stockUpdate.ProductId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing stock update for product {ProductId}", 
                stockUpdate.ProductId);
        }
    }
}
