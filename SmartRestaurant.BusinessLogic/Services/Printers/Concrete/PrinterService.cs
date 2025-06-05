using Microsoft.EntityFrameworkCore;
using SmartRestaurant.BusinessLogic.Services.Printers.DTOs;
using SmartRestaurant.DataAccess.Interfaces;
using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.Printers.Concrete;

public class PrinterService : IPrinterService
{
    private readonly IUnitOfWork _unitOfWork;

    public PrinterService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<PrinterDto>> GetAllAsync()
    {
        var Printers = await _unitOfWork.Printers
                                        .GetAll().ToListAsync();

        return Printers
                    .Select(t => (PrinterDto)t)
                    .ToList();

    }

    public async Task<PrinterDto?> GetByIdAsync(Guid id)
    {
        var Printer = await _unitOfWork.Printers.GetByIdAsync(id);
        return Printer is null ? null : (PrinterDto)Printer;
    }

    public async Task<bool> CreateAsync(AddPrinterDto dto)
    {
        var entity = (Printer)dto;
        return await _unitOfWork.Printers.AddAsync(entity);
    }

    public async Task<bool> UpdateAsync(PrinterDto dto)
    {
        var entity = await _unitOfWork.Printers.GetByIdAsync(dto.Id);
        if (entity == null) return false;

        entity.Name = dto.Name;
        await _unitOfWork.Printers.UpdateAsync(entity);
        return await _unitOfWork.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _unitOfWork.Printers.DeleteByIdAsync(id);
    }
}
