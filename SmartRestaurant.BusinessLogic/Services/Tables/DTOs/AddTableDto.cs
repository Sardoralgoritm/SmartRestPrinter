using SmartRestaurant.Domain.Const;
using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.Tables.DTOs;

public class AddTableDto
{
    public string Name { get; set; } = string.Empty;
    public Guid? TableCategoryId { get; set; }

    public static explicit operator Table(AddTableDto dto)
    {
        return new Table
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Status = TableStatus.Free,
            TableCategoryId = dto.TableCategoryId,
            IsDeleted = false
        };
    }
}
