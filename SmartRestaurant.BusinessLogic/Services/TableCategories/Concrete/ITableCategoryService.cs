using SmartRestaurant.BusinessLogic.Services.Categories.DTOs;
using SmartRestaurant.BusinessLogic.Services.TableCategories.DTOs;

namespace SmartRestaurant.BusinessLogic.Services.TableCategories.Concrete;

public interface ITableCategoryService
{
    Task<List<TableCategoryDto>> GetAllAsync();
    Task<TableCategoryDto?> GetByIdAsync(Guid id);
    Task<bool> CreateAsync(AddTableCategoryDto tableCategory);
    Task<bool> UpdateAsync(TableCategoryDto tableCategory);
    Task<bool> DeleteAsync(Guid id);
}
