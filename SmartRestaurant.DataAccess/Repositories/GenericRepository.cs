using Microsoft.EntityFrameworkCore;
using SmartRestaurant.DataAccess.Data;
using SmartRestaurant.DataAccess.Interfaces;
using SmartRestaurant.Domain.Entities;
using System.Linq.Expressions;

namespace SmartRestaurant.DataAccess.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().Where(i => i.IsDeleted == false).ToListAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync();
    }

    public async Task<bool> AddAsync(T entity)
    {
        try
        {
            await _dbSet.AddAsync(entity);

            return await _context.SaveChangesAsync() > 0;
        }
        catch
        {
            _context.ChangeTracker.Clear();
            return false;
        }
    }

    public async Task<bool> AddRangeAsync(IEnumerable<T> entities)
    {
        try
        {
            await _dbSet.AddRangeAsync(entities);
            return true;
        }
        catch
        {
            _context.ChangeTracker.Clear();
            return false;
        }
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        try
        {
            var existingEntity = await _dbSet.FindAsync(entity.Id);
            if (existingEntity == null) return false;

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            return true;
        }
        catch
        {
            _context.ChangeTracker.Clear();
            return false;
        }
    }

    public async Task<bool> DeleteAsync(T entity)
    {
        try
        {
            var existingEntity = await _dbSet.FindAsync(entity.Id);
            if (existingEntity == null)
                return false;
            entity.IsDeleted = true;
            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            return true;
        }
        catch
        {
            _context.ChangeTracker.Clear();
            return false;
        }
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        try
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
                return false;

            entity.IsDeleted = true;
            _context.Entry(entity).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            _context.ChangeTracker.Clear();
            return false;
        }
    }

    public IQueryable<T> GetAll()
    {
        return _dbSet.AsNoTracking().Where(i => i.IsDeleted == false);
    }
}
