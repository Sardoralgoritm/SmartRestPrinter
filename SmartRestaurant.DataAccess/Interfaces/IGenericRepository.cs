using System.Linq.Expressions;

namespace SmartRestaurant.DataAccess.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    IQueryable<T> GetAll();
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AddAsync(T entity);
    Task<bool> AddRangeAsync(IEnumerable<T> entities);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(T entity);
    Task<bool> DeleteByIdAsync(Guid id);
}
