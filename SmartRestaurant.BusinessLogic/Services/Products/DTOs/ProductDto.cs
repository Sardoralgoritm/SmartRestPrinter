using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.Products.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public double Price { get; set; }
    public string CategoryName { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public string ImagePath { get; set; } = null!;
    public string PrinterName { get; set; } = null!;
    public bool IsActive { get; set; } = true;

    public static explicit operator ProductDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            ImagePath = product.ImagePath,
            CategoryName = product.Category?.Name ?? string.Empty,
            CategoryId = product.CategoryId,
            PrinterName = product.PrinterName,
            IsActive = product.IsActive
        };
    }

    public static explicit operator Product(ProductDto productDto)
    {
        return new Product
        {
            Id = productDto.Id,
            Name = productDto.Name,
            Price = productDto.Price,
            ImagePath = productDto.ImagePath,
            CategoryId = productDto.CategoryId,
            PrinterName = productDto.PrinterName,
            IsActive = productDto.IsActive
        };
    }
}
