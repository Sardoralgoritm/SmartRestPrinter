using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.OrderItems.DTOs;

public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public double Quantity { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public double ProductPrice { get; set; }
    public double TotalPrice => Quantity * ProductPrice;
    public string CategoryName { get; set; } = string.Empty;
    public string PrinterName { get; set; } = string.Empty;
    public static implicit operator OrderItemDto(OrderItem entity)
    {
        return new OrderItemDto
        {
            Id = entity.Id,
            OrderId = entity.OrderId,
            Quantity = entity.Quantity,
            CategoryName = entity.Product.Category.Name,
            ProductId = entity.Product.Id,
            ProductName = entity.Product?.Name ?? "Unknown",
            ProductPrice = entity.ProductPrice,
            PrinterName = entity.Product?.PrinterName ?? "Unknown"
        };
    }

    public static explicit operator OrderItem(OrderItemDto dto)
    {
        return new OrderItem
        {
            Id = dto.Id,
            OrderId = dto.OrderId,
            Quantity = dto.Quantity,
            ProductPrice = dto.ProductPrice,
            ProductId = dto.ProductId
        };
    }
}
