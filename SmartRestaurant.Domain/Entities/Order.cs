using System.ComponentModel.DataAnnotations.Schema;

namespace SmartRestaurant.Domain.Entities;

[Table("order")]
public class Order : BaseEntity
{
    [Column("queue_number")]
    public int QueueNumber { get; set; }

    [Column("status")]
    public string Status { get; set; } = "Open";

    [Column("total_price")]
    public double TotalPrice { get; set; }

    [Column("trsansaction_id")]
    public string TransactionId { get; set; } = string.Empty;

    [Column("cient_phone_number")]
    public string ClientPhoneNumber { get; set; } = string.Empty;

    [Column("canceled_at")]
    public DateTime? CanceledAt { get; set; }

    [Column("table_id")]
    public Guid TableId { get; set; }

    [ForeignKey("TableId")]
    public Table Table { get; set; } = null!;

    [Column("ordered_by_user_id")]
    public Guid OrderedByUserId { get; set; }

    [ForeignKey("OrderedByUserId")]
    public User OrderedByUser { get; set; } = null!;

    [Column("closed_by_user_id")]
    public Guid? ClosedByUserId { get; set; }

    [ForeignKey("ClosedByUserId")]
    public User? ClosedByUser { get; set; }

    [Column("canceled_by_user_id")]
    public Guid? CanceledByUserId { get; set; }

    [ForeignKey("CanceledByUserId")]
    public User? CanceledByUser { get; set; }


    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
