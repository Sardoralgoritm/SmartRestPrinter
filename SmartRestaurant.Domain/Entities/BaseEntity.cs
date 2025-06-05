using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SmartRestaurant.Domain.Const;

namespace SmartRestaurant.Domain.Entities;

public class BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTimeUzb.UZBTime;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTimeUzb.UZBTime;

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;
}
