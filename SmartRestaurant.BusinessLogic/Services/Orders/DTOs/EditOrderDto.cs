using SmartRestaurant.BusinessLogic.Services.OrderItems.DTOs;

namespace SmartRestaurant.BusinessLogic.Services.Orders.DTOs;

public class EditOrderDto
{
    public Guid OrderId { get; set; }
    public Guid ClosedUserId { get; set; }
    public string ClientPhoneNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
}
