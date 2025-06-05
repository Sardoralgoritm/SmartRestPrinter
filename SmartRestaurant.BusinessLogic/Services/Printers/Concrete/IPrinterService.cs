using SmartRestaurant.BusinessLogic.Services.Printers.DTOs;

namespace SmartRestaurant.BusinessLogic.Services.Printers.Concrete;

public interface IPrinterService
{
    Task<List<PrinterDto>> GetAllAsync();
    Task<PrinterDto?> GetByIdAsync(Guid id);
    Task<bool> CreateAsync(AddPrinterDto dto);
    Task<bool> UpdateAsync(PrinterDto dto);
    Task<bool> DeleteAsync(Guid id);
}
