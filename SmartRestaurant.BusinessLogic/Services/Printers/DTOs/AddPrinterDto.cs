using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.Printers.DTOs;

public class AddPrinterDto
{
    public string Name { get; set; } = string.Empty;

    public static explicit operator Printer(AddPrinterDto dto)
    {
        return new Printer
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            IsDeleted = false
        };
    }
}
