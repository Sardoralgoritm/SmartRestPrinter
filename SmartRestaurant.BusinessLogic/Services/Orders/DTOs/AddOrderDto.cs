using SmartRestaurant.BusinessLogic.Services.OrderItems.DTOs;

namespace SmartRestaurant.BusinessLogic.Services.Orders.DTOs;

public class AddOrderDto
{
    public Guid TableId { get; set; }
    public Guid OrderedUserId { get; set; }
    public Guid? ClosedUserId { get; set; }
    public string? Status { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string ClientPhoneNumber { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
}
