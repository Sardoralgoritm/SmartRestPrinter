using SmartRestaurant.BusinessLogic.Services.OrderItems.DTOs;
using SmartRestaurant.DataAccess.Interfaces;
using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.OrderItems.Concrete;

public class OrderItemService : IOrderItemService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderItemService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<OrderItemDto>> GetAllAsync()
    {
        var items = await _unitOfWork.OrderItems.GetAllAsync();
        return items.Select(x => (OrderItemDto)x).ToList();
    }

    public async Task<OrderItemDto?> GetByIdAsync(Guid id)
    {
        var item = await _unitOfWork.OrderItems.GetByIdAsync(id);
        return item is null ? null : (OrderItemDto)item;
    }

    public async Task<bool> AddAsync(AddOrderItemDto dto, Guid orderId)
    {
        var entity = (OrderItem)dto;
        entity.Id = Guid.NewGuid();
        entity.OrderId = orderId;

        return await _unitOfWork.OrderItems.AddAsync(entity);
    }

    public async Task<bool> UpdateAsync(OrderItemDto dto)
    {
        var entity = await _unitOfWork.OrderItems.GetByIdAsync(dto.Id);
        if (entity == null) return false;

        entity.Quantity = dto.Quantity;
        return await _unitOfWork.OrderItems.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _unitOfWork.OrderItems.GetByIdAsync(id);
        if (entity == null) return false;
        entity.Quantity = 0;
        var res = await _unitOfWork.OrderItems.DeleteAsync(entity);
        if (res)
        {
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<bool> AddManyAsync(List<OrderItem> items)
    {
        await _unitOfWork.OrderItems.AddRangeAsync(items);
        var result = await _unitOfWork.SaveChangesAsync();
        return result > 0;
    }
}
