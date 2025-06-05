using SmartRestaurant.BusinessLogic.Services.Categories.DTOs;
using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.TableCategories.DTOs;

public class TableCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public static implicit operator TableCategoryDto(TableCategory entity)
    {
        return new TableCategoryDto
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }

    public static implicit operator TableCategory(TableCategoryDto dto)
    {
        return new TableCategory
        {
            Id = dto.Id,
            Name = dto.Name
        };
    }
}
