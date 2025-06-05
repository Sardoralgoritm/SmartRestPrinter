using SmartRestaurant.BusinessLogic.Services.Categories.DTOs;
using SmartRestaurant.DataAccess.Interfaces;
using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        return categories.Select(c => (CategoryDto)c).ToList();
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        return category is null ? null : (CategoryDto)category;
    }

    public async Task<bool> CreateAsync(AddCategoryDto categoryDto)
    {
        var category = (Category)categoryDto;
        return await _unitOfWork.Categories.AddAsync(category);
    }

    public async Task<bool> UpdateAsync(CategoryDto categoryDto)
    {
        var category = (Category)categoryDto;
        await _unitOfWork.Categories.UpdateAsync(category);
        return await _unitOfWork.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _unitOfWork.Categories.DeleteByIdAsync(id);
    }

    public async Task<bool> IsDeletingPossibleAsync(Guid id)
    {
        var category = await _unitOfWork.Categories.IsDeletingPossible(id);

        if (category is null) return false;

        if (category.Products.Any(p => !p.IsDeleted)) return false;

        return true;
    }
}
