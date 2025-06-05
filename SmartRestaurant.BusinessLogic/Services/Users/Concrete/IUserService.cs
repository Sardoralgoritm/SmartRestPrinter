using SmartRestaurant.BusinessLogic.Services.Users.DTOs;

namespace SmartRestaurant.BusinessLogic.Services.Users.Concrete;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto> GetUserByIdAsync(Guid id);
    Task<bool> UpdateUserAsync(UserDto user);
    Task<bool> DeleteUserAsync(Guid id);
}
