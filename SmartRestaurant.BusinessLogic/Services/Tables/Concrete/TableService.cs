using Microsoft.EntityFrameworkCore;
using SmartRestaurant.BusinessLogic.Services.Tables.DTOs;
using SmartRestaurant.BusinessLogic.Services.Tables.QueryObjects;
using SmartRestaurant.DataAccess.Interfaces;
using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.Tables.Concrete;

public class TableService : ITableService
{
    private readonly IUnitOfWork _unitOfWork;

    public TableService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<TableDto>> GetAllAsync(TableSortFilterOption option)
    {
        var tables = await _unitOfWork.Tables
                                        .GetAll()
                                        .Include(t => t.TableCategory)
                                        .Where(t => !t.Name.ToLower().StartsWith("takeaway"))
                                        .SortFilter(option)
                                        .ToListAsync();

        return tables
                    .Select(t => (TableDto)t)
                    .OrderBy(i => int.Parse(i.Name.Split()[1]))
                    .ToList();

    }

    public async Task<TableDto?> GetByIdAsync(Guid id)
    {
        var table = await _unitOfWork.Tables.GetByIdAsync(id);
        return table is null ? null : (TableDto)table;
    }

    public async Task<bool> CreateAsync(AddTableDto dto)
    {
        var entity = (Table)dto;
        return await _unitOfWork.Tables.AddAsync(entity);
    }

    public async Task<bool> UpdateAsync(TableDto dto)
    {
        var entity = await _unitOfWork.Tables.GetByIdAsync(dto.Id);
        if (entity == null) return false;

        entity.Name = dto.Name;
        entity.Status = dto.Status;
        await _unitOfWork.Tables.UpdateAsync(entity);
        return await _unitOfWork.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _unitOfWork.Tables.DeleteByIdAsync(id);
    }

    public Task<Table?> GetTakeAwayAsync()
    {
        return _unitOfWork.Tables.GetAll()
            .FirstOrDefaultAsync(t => t.Name.ToLower().StartsWith("takeaway"));
    }
}
