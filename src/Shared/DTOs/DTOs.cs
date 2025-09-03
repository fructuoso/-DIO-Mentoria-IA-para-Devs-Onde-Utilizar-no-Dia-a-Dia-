using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}

public class CreateProductDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }
    
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be non-negative")]
    public int StockQuantity { get; set; }
}

public class UpdateProductDto
{
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal? Price { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be non-negative")]
    public int? StockQuantity { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class CreateOrderDto
{
    [Required]
    public string CustomerId { get; set; } = string.Empty;
    
    [Required]
    [MinLength(1, ErrorMessage = "Order must have at least one item")]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CreateOrderItemDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Product ID must be valid")]
    public int ProductId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
}

public class StockUpdateDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
