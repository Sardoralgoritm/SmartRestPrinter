using System.ComponentModel.DataAnnotations.Schema;

namespace SmartRestaurant.Domain.Entities;

[Table("order_item")]
public class OrderItem : BaseEntity
{
    [Column("quantity")]
    public double Quantity { get; set; }

    [Column("product_price")]
    public double ProductPrice { get; set; }

    [Column("order_id")]
    public Guid OrderId { get; set; }

    [ForeignKey("OrderId")]
    public Order Order { get; set; } = null!;

    [Column("product_id")]
    public Guid ProductId { get; set; }

    [ForeignKey("ProductId")]
    public Product Product { get; set; } = null!;
}

