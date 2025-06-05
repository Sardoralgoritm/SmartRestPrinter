using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.Categories.DTOs;

public class AddCategoryDto
{
    public string Name { get; set; } = string.Empty;

    public static explicit operator Category(AddCategoryDto dto)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            Name = dto.Name
        };
    }
}
