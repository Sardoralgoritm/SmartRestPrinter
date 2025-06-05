using SmartRestaurant.BusinessLogic.Extentions.Security;
using SmartRestaurant.BusinessLogic.Services.Auth.DTOs;
using SmartRestaurant.BusinessLogic.Services.Users.DTOs;
using SmartRestaurant.DataAccess.Interfaces;
using SmartRestaurant.Domain.Const;
using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.Auth.Concrete;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public void ClearTracking()
    {
        _unitOfWork.ClearTracking();
    }

    public async Task<UserDto?> LoginAsync(UserLoginDto userLoginDto)
    {
        var users = await _unitOfWork.Users.GetAllAsync();

        foreach (var user in users)
        {
            if (PasswordHasher.Verify(userLoginDto.Password!, user.PasswordSalt, user.PasswordHash))
            {
                return new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    ImageUrl = user.ImageUrl,
                    Role = user.Role
                };
            }
        }

        return null;
    }

    public async Task<bool> RegisterAsync(UserRegisterDto userRegisterDto)
    {
        try
        {
            var isExist = await _unitOfWork.Users.GetByPhoneNumberAsync(userRegisterDto.PhoneNumber);

            if (isExist != null)
            {
                return false;
            }

            (string Hash, string Salt) = PasswordHasher.Hash(userRegisterDto.Password!);

            var newUser = new User
            {
                FirstName = userRegisterDto.FirstName,
                LastName = userRegisterDto.LastName,
                PhoneNumber = userRegisterDto.PhoneNumber,
                ImageUrl = userRegisterDto.ImageUrl,
                Role = userRegisterDto.Role,
                PasswordHash = Hash,
                PasswordSalt = Salt
            };

            var res = await _unitOfWork.Users.AddAsync(newUser);

            return res;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> IsPhoneNumberExistsAsync(string phoneNumber)
    {
        return await _unitOfWork.Users.IsPhoneNumberExistsAsync(phoneNumber);
    }

    public async Task<bool> IsPasswordExistAsync(string password)
    {
        var users = await _unitOfWork.Users.GetAllAsync();

        foreach (var user in users)
        {
            if (PasswordHasher.Verify(password, user.PasswordSalt, user.PasswordHash))
            {
                return true;
            }
        }

        return false;
    }

    public bool CheckBossPassword(string password)
    {
        var bossUsers = _unitOfWork.Users.GetAll().Where(u => u.Role == Roles.BOSS);

        foreach (var user in bossUsers)
        {
            var isMatch = PasswordHasher.Verify(password, user.PasswordSalt, user.PasswordHash);
            if (isMatch)
                return true;
        }

        return false;
    }
}
