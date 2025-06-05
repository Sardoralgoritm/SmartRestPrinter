using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.Products.DTOs;

public class AddProductDto
{
    public string Name { get; set; } = null!;
    public double Price { get; set; }
    public Guid CategoryId { get; set; }
    public string ImagePath { get; set; } = null!;
    public string PrinterName { get; set; } = null!;

    public static implicit operator Product(AddProductDto dto)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            ImagePath = dto.ImagePath,
            PrinterName = dto.PrinterName,
            IsActive = true,
        };
    }
}

