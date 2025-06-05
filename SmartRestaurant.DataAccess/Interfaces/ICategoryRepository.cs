using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.DataAccess.Interfaces;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<Category?> IsDeletingPossible(Guid id);
}
