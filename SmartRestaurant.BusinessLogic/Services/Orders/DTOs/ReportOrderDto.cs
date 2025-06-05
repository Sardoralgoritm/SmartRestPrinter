using SmartRestaurant.BusinessLogic.Services.OrderItems.DTOs;

namespace SmartRestaurant.BusinessLogic.Services.Orders.DTOs;

public class ReportOrderDto
{
    public int TotalOrdersCount { get; set; } = 0;
    public double TotalRevenue { get; set; } = 0;
    public List<OrderItemDto> TopOrderItems { get; set; } = new List<OrderItemDto>();
}
