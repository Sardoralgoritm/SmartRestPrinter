using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.DataAccess.Interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<Order?> GetOpenOrderWithProductsByTableIdAsync(Guid tableId);
}
