using Microsoft.EntityFrameworkCore;
using SmartRestaurant.BusinessLogic.Extentions;
using SmartRestaurant.BusinessLogic.Services.Products.DTOs;
using SmartRestaurant.DataAccess.Interfaces;
using SmartRestaurant.Domain.Entities;
using SmartRestaurant.Domain.Models.PageResult;

namespace SmartRestaurant.BusinessLogic.Services.Products.Concrete;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public PagedResult<ProductDto> GetAll(ProductSortFilterOptions options)
    {
        var products = _unitOfWork.Products.GetAll().Include(p => p.Category).SortFilter(options);
        return products.Select(p => (ProductDto)p).AsPagedResult(options);
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        return product is null ? null : (ProductDto)product;
    }

    public async Task<bool> AddAsync(AddProductDto dto)
    {
        var entity = (Product)dto;
        return await _unitOfWork.Products.AddAsync(entity);
    }

    public async Task<bool> UpdateAsync(ProductDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(dto.Id);
        if (product is null) return false;

        var updatedProduct = (Product)dto;

        await _unitOfWork.Products.UpdateAsync(updatedProduct);

        return await _unitOfWork.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _unitOfWork.Products.DeleteByIdAsync(id);
    }

    public async Task<List<ProductDto>> GetAllAsync(ProductSortFilterOptions options)
    {
        var products = await _unitOfWork.Products.GetAll()
            .Include(p => p.Category)
            .SortFilter(options)
            .ToListAsync();
        return products.Select(p => (ProductDto)p).ToList();
    }
}
