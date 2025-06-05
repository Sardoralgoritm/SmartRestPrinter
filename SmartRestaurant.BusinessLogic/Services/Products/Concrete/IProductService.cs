using SmartRestaurant.BusinessLogic.Services.Products.DTOs;
using SmartRestaurant.Domain.Models.PageResult;

namespace SmartRestaurant.BusinessLogic.Services.Products.Concrete;

public interface IProductService
{
    PagedResult<ProductDto> GetAll(ProductSortFilterOptions options);
    Task<List<ProductDto>> GetAllAsync(ProductSortFilterOptions options);
    Task<ProductDto?> GetByIdAsync(Guid id);
    Task<bool> AddAsync(AddProductDto dto);
    Task<bool> UpdateAsync(ProductDto dto);
    Task<bool> DeleteAsync(Guid id);
}

