using System.ComponentModel.DataAnnotations.Schema;

namespace SmartRestaurant.Domain.Entities;

[Table("table_category")]
public class TableCategory : BaseEntity
{
    [Column("name")]
    public string Name { get; set; } = null!;

    public ICollection<Table> Tables { get; set; } = new List<Table>();
}
