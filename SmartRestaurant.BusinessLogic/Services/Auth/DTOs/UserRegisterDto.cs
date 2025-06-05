using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.Auth.DTOs;

public class UserRegisterDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;


    public static explicit operator User(UserRegisterDto userRegisterDto)
    {
        return new User
        {
            FirstName = userRegisterDto.FirstName,
            LastName = userRegisterDto.LastName,
            ImageUrl = userRegisterDto.ImageUrl,
            PhoneNumber = userRegisterDto.PhoneNumber,
            Role = userRegisterDto.Role
        };
    }
}
