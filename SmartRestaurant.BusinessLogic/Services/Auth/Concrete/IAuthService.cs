using SmartRestaurant.BusinessLogic.Services.Auth.DTOs;
using SmartRestaurant.BusinessLogic.Services.Users.DTOs;

namespace SmartRestaurant.BusinessLogic.Services.Auth.Concrete;

public interface IAuthService
{
    Task<UserDto?> LoginAsync(UserLoginDto userLoginDto);
    Task<bool> RegisterAsync(UserRegisterDto userRegisterDto);
    Task<bool> IsPhoneNumberExistsAsync(string phoneNumber);
    void ClearTracking();
    Task<bool> IsPasswordExistAsync(string password);
    bool CheckBossPassword(string password);
}
