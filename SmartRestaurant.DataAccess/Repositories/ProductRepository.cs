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
}