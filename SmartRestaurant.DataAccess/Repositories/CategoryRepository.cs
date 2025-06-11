using Microsoft.EntityFrameworkCore;
using SmartRestaurant.DataAccess.Data;
using SmartRestaurant.DataAccess.Interfaces;
using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.DataAccess.Repositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    private readonly AppDbContext _context;
    public CategoryRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Category?> IsDeletingPossible(Guid id)
    {
        return await _context.Categories.Include(c => c.Products).AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
    }

    //private readonly IDbContextFactory<AppDbContext> _contextFactory;

    //public CategoryRepository(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
    //{
    //    _contextFactory = contextFactory;
    //}

    //public async Task<Category?> IsDeletingPossible(Guid id)
    //{
    //    using var context = _contextFactory.CreateDbContext();
    //    return await context.Categories
    //        .Include(c => c.Products)
    //        .AsNoTracking()
    //        .FirstOrDefaultAsync(c => c.Id == id);
    //}
}
