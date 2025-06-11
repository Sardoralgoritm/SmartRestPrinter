using Microsoft.EntityFrameworkCore;
using SmartRestaurant.DataAccess.Data;
using SmartRestaurant.DataAccess.Interfaces;
using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.DataAccess.Repositories;

public class TableCategoryRepository : GenericRepository<TableCategory>, ITableCategoryRepository
{
    public TableCategoryRepository(AppDbContext context) : base(context)
    { }

    //private readonly IDbContextFactory<AppDbContext> _contextFactory;
    //public TableCategoryRepository(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
    //{
    //    _contextFactory = contextFactory;
    //}
}
