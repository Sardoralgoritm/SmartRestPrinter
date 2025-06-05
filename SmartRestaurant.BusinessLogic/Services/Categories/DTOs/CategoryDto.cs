using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.Categories.DTOs;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public static implicit operator CategoryDto(Category entity)
    {
        return new CategoryDto
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }

    public static implicit operator Category(CategoryDto dto)
    {
        return new Category
        {
            Id = dto.Id,
            Name = dto.Name
        };
    }
}
