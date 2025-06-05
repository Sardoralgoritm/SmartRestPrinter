using System.ComponentModel.DataAnnotations.Schema;

namespace SmartRestaurant.Domain.Entities;

[Table("category")]
public class Category : BaseEntity
{
    [Column("name")]
    public string Name { get; set; } = null!;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
