using SmartRestaurant.BusinessLogic.Services.OrderItems.DTOs;
using SmartRestaurant.Domain.Const;
using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.Orders.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid TableId { get; set; }
    public Guid UserId { get; set; }
    public int QueueNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CanceledAt { get; set; }
    public string Status { get; set; } = OrderStatus.Open;
    public string TableName { get; set; } = string.Empty;
    public string OrderedUserName { get; set; } = string.Empty;
    public string ClosedUserName { get; set; } = string.Empty;
    public string? CanceledUserName { get; set; }
    public double TotalPrice { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();

    public static implicit operator OrderDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            TableId = order.TableId,
            TableName = order.Table.Name.ToLower().StartsWith("takeaway") ? "Olib ketish" : order.Table.Name,
            TotalPrice = order.TotalPrice,
            QueueNumber = order.QueueNumber,
            CreatedAt = order.CreatedAt,
            Status = order.Status,
            Items = order.OrderItems.Select(i => (OrderItemDto)i).ToList(),
            TransactionId = order.TransactionId,
            OrderedUserName = order.OrderedByUser.FirstName,
            ClosedUserName = order.ClosedByUser?.FirstName ?? string.Empty,
            CanceledUserName = order.CanceledByUser?.FirstName ?? string.Empty,
            CanceledAt = order.CanceledAt,
            UpdatedAt = order.UpdatedAt
        };
    }

    public static implicit operator Order(OrderDto orderDto)
    {
        return new Order
        {
            Id = orderDto.Id,
            TableId = orderDto.TableId,
            QueueNumber = orderDto.QueueNumber,
            CreatedAt = orderDto.CreatedAt,
            Status = orderDto.Status,
            TotalPrice = orderDto.TotalPrice,
            OrderItems = orderDto.Items.Select(i => (OrderItem)i).ToList(),
            TransactionId = orderDto.TransactionId,
            OrderedByUserId = orderDto.UserId
        };
    }
}