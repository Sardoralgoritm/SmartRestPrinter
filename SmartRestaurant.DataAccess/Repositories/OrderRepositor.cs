using Microsoft.EntityFrameworkCore;
using SmartRestaurant.DataAccess.Data;
using SmartRestaurant.DataAccess.Interfaces;
using SmartRestaurant.Domain.Const;
using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.DataAccess.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Order?> GetOpenOrderWithProductsByTableIdAsync(Guid tableId)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => !o.IsDeleted)
            .Include(o => o.OrderItems
                .Where(oi => !oi.IsDeleted))
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Category)
            .Include(o => o.OrderedByUser)
            .Include(o => o.ClosedByUser)
            .Include(o => o.Table)
            .FirstOrDefaultAsync(o => o.TableId == tableId && o.Status == OrderStatus.Open);
    }

    //private readonly IDbContextFactory<AppDbContext> _contextFactory;
    //public OrderRepository(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
    //{
    //    _contextFactory = contextFactory;
    //}

    //public async Task<Order?> GetOpenOrderWithProductsByTableIdAsync(Guid tableId)
    //{
    //    using var context = _contextFactory.CreateDbContext();
    //    return await context.Orders
    //        .AsNoTracking()
    //        .Where(o => !o.IsDeleted)
    //        .Include(o => o.OrderItems
    //            .Where(oi => !oi.IsDeleted))
    //            .ThenInclude(oi => oi.Product)
    //                .ThenInclude(p => p.Category)
    //        .Include(o => o.OrderedByUser)
    //        .Include(o => o.ClosedByUser)
    //        .Include(o => o.Table)
    //        .FirstOrDefaultAsync(o => o.TableId == tableId && o.Status == OrderStatus.Open);
    //}
}

