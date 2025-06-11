using Microsoft.EntityFrameworkCore;
using SmartRestaurant.DataAccess.Data;
using SmartRestaurant.DataAccess.Interfaces;
using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.DataAccess.Repositories;

public class TableRepository : GenericRepository<Table>, ITableRepository
{
    public TableRepository(AppDbContext context) : base(context) { }
    //private readonly IDbContextFactory<AppDbContext> _contextFactory;
    //public TableRepository(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
    //{
    //    _contextFactory = contextFactory;
    //}
}
