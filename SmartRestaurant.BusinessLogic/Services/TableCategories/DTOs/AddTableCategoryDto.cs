using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.TableCategories.DTOs;

public class AddTableCategoryDto
{
    public string Name { get; set; } = string.Empty;

    public static explicit operator TableCategory(AddTableCategoryDto dto)
    {
        return new TableCategory
        {
            Id = Guid.NewGuid(),
            Name = dto.Name
        };
    }
}
