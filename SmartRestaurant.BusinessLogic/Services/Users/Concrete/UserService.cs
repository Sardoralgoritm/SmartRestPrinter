using Microsoft.EntityFrameworkCore;
using SmartRestaurant.BusinessLogic.Extentions.Security;
using SmartRestaurant.BusinessLogic.Services.Users.DTOs;
using SmartRestaurant.DataAccess.Interfaces;

namespace SmartRestaurant.BusinessLogic.Services.Users.Concrete;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.Users.GetAll().ToListAsync();
        return users.Select(u => (UserDto)u).ToList();
    }

    public async Task<UserDto> GetUserByIdAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        return user is null ? null! : (UserDto)user;
    }

    public async Task<bool> UpdateUserAsync(UserDto userDto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userDto.Id);
        if (user is null) return false;

        if (userDto.Password!.Length > 0)
        {
            (string Hash, string Salt) = PasswordHasher.Hash(userDto.Password);
            user.PasswordHash = Hash;
            user.PasswordSalt = Salt;
        }

        user.FirstName = userDto.FirstName;
        user.LastName = userDto.LastName;
        user.ImageUrl = userDto.ImageUrl;
        user.PhoneNumber = userDto.PhoneNumber;
        user.Role = userDto.Role;

        await _unitOfWork.Users.UpdateAsync(user);
        return await _unitOfWork.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user is null) return false;

        user.PhoneNumber = null;
        await _unitOfWork.Users.DeleteAsync(user);
        return await _unitOfWork.SaveChangesAsync() > 0;
    }
}

