using Microsoft.EntityFrameworkCore;
using SmartRestaurant.DataAccess.Data;
using SmartRestaurant.DataAccess.Interfaces;
using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.DataAccess.Repositories;

public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
{
    private readonly AppDbContext _context;

    public OrderItemRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public void RemoveRange(IEnumerable<OrderItem> entities)
    {
        foreach (var entity in entities)
        {
            var localEntity = _dbSet.Local.FirstOrDefault(e => e.Id == entity.Id);
            if (localEntity != null)
            {
                _context.Entry(localEntity).State = EntityState.Deleted;
            }
            else
            {
                _dbSet.Attach(entity);
                _dbSet.Remove(entity);
            }
        }
    }

    //private readonly IDbContextFactory<AppDbContext> _contextFactory;
    //public OrderItemRepository(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
    //{
    //    _contextFactory = contextFactory;
    //}

    //public void RemoveRange(IEnumerable<OrderItem> entities)
    //{
    //    using var context = _contextFactory.CreateDbContext();
    //    foreach (var entity in entities)
    //    {
    //        var localEntity = context.Set<OrderItem>().Local.FirstOrDefault(e => e.Id == entity.Id);
    //        if (localEntity != null)
    //        {
    //            context.Entry(localEntity).State = EntityState.Deleted;
    //        }
    //        else
    //        {
    //            context.Set<OrderItem>().Attach(entity);
    //            context.Set<OrderItem>().Remove(entity);
    //        }
    //    }
    //}
}
