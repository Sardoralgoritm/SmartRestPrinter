using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.DataAccess.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<bool> IsPhoneNumberExistsAsync(string phoneNumber);
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
}

