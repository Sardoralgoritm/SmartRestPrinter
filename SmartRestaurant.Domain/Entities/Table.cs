using ST = SmartRestaurant.Domain.Const;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartRestaurant.Domain.Entities;

[Table("table")]
public class Table : BaseEntity
{
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("status")]
    public string Status { get; set; } = ST.TableStatus.Free;

    [Column("table_category_id")]
    public Guid? TableCategoryId { get; set; }

    [ForeignKey("TableCategoryId")]
    public TableCategory? TableCategory { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
