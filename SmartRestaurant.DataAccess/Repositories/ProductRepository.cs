using Microsoft.EntityFrameworkCore;
using SmartRestaurant.DataAccess.Data;
using SmartRestaurant.DataAccess.Interfaces;
using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.DataAccess.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    private readonly AppDbContext _context;
    public ProductRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllWithEntityAsync()
    {
        return await _context.Products.Include(P => P.Category).AsNoTracking().ToListAsync();
    }

    //private readonly IDbContextFactory<AppDbContext> _contextFactory;
    //public ProductRepository(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
    //{
    //    _contextFactory = contextFactory;
    //}
    //public async Task<List<Product>> GetAllWithEntityAsync()
    //{
    //    using var context = _contextFactory.CreateDbContext();
    //    return await context.Products
    //        .Include(p => p.Category)
    //        .AsNoTracking()
    //        .ToListAsync();
    //}
}