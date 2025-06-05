using SmartRestaurant.Domain.Const;
using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.Tables.DTOs;

public class TableDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = TableStatus.Free;

    public static implicit operator TableDto(Table entity)
    {
        return new TableDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Status = entity.Status
        };
    }
}