using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.DataAccess.Interfaces;

public interface IOrderItemRepository : IGenericRepository<OrderItem>
{
    void RemoveRange(IEnumerable<OrderItem> entities);
}
