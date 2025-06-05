using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.Printers.DTOs;

public class PrinterDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public static implicit operator PrinterDto(Printer entity)
    {
        return new PrinterDto
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }
}