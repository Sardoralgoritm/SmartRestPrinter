using System.ComponentModel.DataAnnotations.Schema;

namespace SmartRestaurant.Domain.Entities;

[Table("product")]
public class Product : BaseEntity
{
    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("price")]
    public double Price { get; set; }

    [Column("image_path")]
    public string ImagePath { get; set; } = null!;

    [Column("printer_name")]
    public string PrinterName { get; set; } = null!;

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("category_id")]
    public Guid CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public Category Category { get; set; } = null!;

    [NotMapped]
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

}
