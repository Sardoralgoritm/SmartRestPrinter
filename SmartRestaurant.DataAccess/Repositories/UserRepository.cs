using Microsoft.EntityFrameworkCore;
using SmartRestaurant.DataAccess.Data;
using SmartRestaurant.DataAccess.Interfaces;
using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.DataAccess.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<User?> GetByPhoneNumberAsync(string phoneNumber)
    {
        return _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
    }

    public async Task<bool> IsPhoneNumberExistsAsync(string phoneNumber)
    {
        return await _context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
    }

    //private readonly IDbContextFactory<AppDbContext> _contextFactory;

    //public UserRepository(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
    //{
    //    _contextFactory = contextFactory;
    //}

    //public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
    //{
    //    using var context = _contextFactory.CreateDbContext();
    //    return await context.Users
    //        .AsNoTracking()
    //        .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
    //}

    //public async Task<bool> IsPhoneNumberExistsAsync(string phoneNumber)
    //{
    //    using var context = _contextFactory.CreateDbContext();
    //    return await context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
    //}
}

