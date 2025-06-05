using SmartRestaurant.BusinessLogic.Services.OrderItems.DTOs;
using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.OrderItems.Concrete;

public interface IOrderItemService
{
    Task<List<OrderItemDto>> GetAllAsync();
    Task<OrderItemDto?> GetByIdAsync(Guid id);
    Task<bool> AddAsync(AddOrderItemDto dto, Guid orderId);
    Task<bool> AddManyAsync(List<OrderItem> items);
    Task<bool> UpdateAsync(OrderItemDto dto);
    Task<bool> DeleteAsync(Guid id);
}