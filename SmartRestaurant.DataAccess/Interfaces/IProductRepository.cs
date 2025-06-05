using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.DataAccess.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<List<Product>> GetAllWithEntityAsync();
}