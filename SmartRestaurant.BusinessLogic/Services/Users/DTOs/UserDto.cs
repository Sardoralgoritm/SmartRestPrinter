using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.Users.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;


    public static explicit operator UserDto(User user)
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