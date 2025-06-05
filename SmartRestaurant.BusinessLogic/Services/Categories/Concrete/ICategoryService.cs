using SmartRestaurant.BusinessLogic.Services.Categories.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRestaurant.BusinessLogic.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(Guid id);
    Task<bool> CreateAsync(AddCategoryDto category);
    Task<bool> UpdateAsync(CategoryDto category);
    Task <bool> IsDeletingPossibleAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
}