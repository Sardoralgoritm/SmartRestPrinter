using System.ComponentModel.DataAnnotations.Schema;

namespace SmartRestaurant.Domain.Entities;

[Table("user")]
public class User : BaseEntity
{
    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Column("image_url")]
    public string ImageUrl { get; set; } = string.Empty;

    [Column("phone_number")]
    public string? PhoneNumber { get; set; }

    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("password_salt")]
    public string PasswordSalt { get; set; } = string.Empty;

    [Column("role")]
    public string Role { get; set; } = string.Empty;


    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

