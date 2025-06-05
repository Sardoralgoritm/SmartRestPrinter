namespace SmartRestaurant.BusinessLogic.Services.Auth.DTOs;

public class UserLoginDto
{
    public string Password { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
