using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.OrderItems.DTOs;

public class AddOrderItemDto
{
    public int Quantity { get; set; }
    public double ProductPrice { get; set; }
    public Guid ProductId { get; set; }

    public static explicit operator OrderItem(AddOrderItemDto dto)
    {
        return new OrderItem
        {
            Id = Guid.NewGuid(),
            Quantity = dto.Quantity,
            ProductPrice = dto.ProductPrice
        };
    }
}
