using SmartRestaurant.BusinessLogic.Services.OrderItems.DTOs;
using SmartRestaurant.BusinessLogic.Services.Orders.DTOs;
using SmartRestaurant.Domain.Models.PageResult;
namespace SmartRestaurant.BusinessLogic.Services.Orders.Concrete;

public interface IOrderService
{
    PagedResult<OrderDto> GetAll(OrderSortFilterOptions option);
    Task<ReportOrderDto> GetReportOrder(OrderSortFilterOptions option);
    PagedResult<OrderItemDto> GetAllReportOrder(OrderSortFilterOptions option);
    Task<List<OrderItemDto>> GetAllReportOrderListAsync(OrderSortFilterOptions option);
    Task<OrderDto?> GetByIdAsync(Guid id);
    Task<(bool, int)> CreateAsync(AddOrderDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> UpdateTotalPrice(Guid orderId, double totalPrice);
    Task<bool> ChangeTableIdAsync(Guid orderId, Guid oldTableId, Guid newTableId);
    Task<OrderDto?> GetOpenOrderByTableId(Guid tableId);
    Task<bool> CloseOrderAndFreeTable(EditOrderDto orderDto, Guid tableId);
    Task<bool> DeleteOrderAndFreeTableAsync(Guid OrderId, Guid tableId);
    Task<bool> CancelOrderAsync(Guid orderId, Guid UserId);
    int GetTodayQueueNumber();
}
