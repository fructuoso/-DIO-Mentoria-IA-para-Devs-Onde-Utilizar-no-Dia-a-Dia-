using Shared.DTOs;
using System.Text.Json;

namespace SalesService.Services;

public class StockService : IStockService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StockService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _gatewayBaseUrl;

    public StockService(HttpClient httpClient, IConfiguration configuration, ILogger<StockService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        // Always communicate through the API Gateway
        _gatewayBaseUrl = configuration["Services:ApiGateway:BaseUrl"] ?? "https://localhost:5000";
    }

    private void SetAuthorizationHeader()
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader))
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", authHeader);
        }
    }

    public async Task<ProductDto?> GetProductAsync(int productId)
    {
        try
        {
            SetAuthorizationHeader();
            
            // Route through API Gateway: /api/stock/products/{id}
            var response = await _httpClient.GetAsync($"{_gatewayBaseUrl}/api/stock/products/{productId}");
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                    
                _logger.LogWarning("Failed to get product {ProductId} via Gateway. Status: {StatusCode}", productId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<ProductDto>(content, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {ProductId} via Gateway", productId);
            return null;
        }
    }

    public async Task<bool> CheckStockAvailabilityAsync(int productId, int quantity)
    {
        try
        {
            SetAuthorizationHeader();
            
            // Route through API Gateway: /api/stock/products/{id}/stock/check/{quantity}
            var response = await _httpClient.GetAsync($"{_gatewayBaseUrl}/api/stock/products/{productId}/stock/check/{quantity}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to check stock availability for product {ProductId} via Gateway. Status: {StatusCode}", productId, response.StatusCode);
                return false;
            }

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<dynamic>(content, options);
            
            return result?.GetProperty("isAvailable").GetBoolean() ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking stock availability for product {ProductId} via Gateway", productId);
            return false;
        }
    }

    public async Task<bool> ReserveStockAsync(int productId, int quantity)
    {
        try
        {
            SetAuthorizationHeader();
            
            var requestContent = new StringContent(
                JsonSerializer.Serialize(quantity),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            // Route through API Gateway: /api/stock/products/{id}/stock/reserve
            var response = await _httpClient.PostAsync($"{_gatewayBaseUrl}/api/stock/products/{productId}/stock/reserve", requestContent);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully reserved {Quantity} units of product {ProductId} via Gateway", quantity, productId);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to reserve stock for product {ProductId} via Gateway. Status: {StatusCode}", productId, response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving stock for product {ProductId} via Gateway", productId);
            return false;
        }
    }
}
