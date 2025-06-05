using SmartRestaurant.BusinessLogic.Services.Tables.DTOs;
using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.Tables.Concrete;

public interface ITableService
{
    Task<List<TableDto>> GetAllAsync(TableSortFilterOption option);
    Task<TableDto?> GetByIdAsync(Guid id);
    Task<bool> CreateAsync(AddTableDto dto);
    Task<bool> UpdateAsync(TableDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<Table?> GetTakeAwayAsync();
}