using SmartRestaurant.BusinessLogic.Services.Categories.DTOs;
using SmartRestaurant.BusinessLogic.Services.TableCategories.DTOs;
using SmartRestaurant.DataAccess.Interfaces;
using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.TableCategories.Concrete;

public class TableCategoryService : ITableCategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public TableCategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<TableCategoryDto>> GetAllAsync()
    {
        var categories = await _unitOfWork.TableCategories.GetAllAsync();
        return categories.Select(c => (TableCategoryDto)c).ToList();
    }

    public async Task<TableCategoryDto?> GetByIdAsync(Guid id)
    {
        var TableCategory = await _unitOfWork.TableCategories.GetByIdAsync(id);
        return TableCategory is null ? null : (TableCategoryDto)TableCategory;
    }

    public async Task<bool> CreateAsync(AddTableCategoryDto tableCategoryDto)
    {
        var TableCategory = (TableCategory)tableCategoryDto;
        return await _unitOfWork.TableCategories.AddAsync(TableCategory);
    }

    public async Task<bool> UpdateAsync(TableCategoryDto tableCategoryDto)
    {
        var TableCategory = (TableCategory)tableCategoryDto;
        await _unitOfWork.TableCategories.UpdateAsync(TableCategory);
        return await _unitOfWork.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _unitOfWork.TableCategories.DeleteByIdAsync(id);
    }
}
