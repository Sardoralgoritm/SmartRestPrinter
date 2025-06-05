using Microsoft.EntityFrameworkCore;
using SmartRestaurant.DataAccess.Data;
using SmartRestaurant.DataAccess.Interfaces;
using SmartRestaurant.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
