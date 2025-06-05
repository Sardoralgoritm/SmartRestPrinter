using SmartRestaurant.Domain.Const;
using SmartRestaurant.Domain.Models.SortFilter;

namespace SmartRestaurant.BusinessLogic.Services;

public class OrderSortFilterOptions : SortFilterPageOptions
{
    public Guid? CategoryId { get; set; }
    public DateTime? FromDateTime { get; set; }
    public DateTime? ToDateTime { get; set; }
    public string? Status { get; set; }
    public Guid? OrderedUserId { get; set; }
    public Guid? ClosedUserId { get; set; }

    public OrderSortFilterOptions()
    {
        FromDateTime ??= DateTimeUzb.UZBTime;
        ToDateTime ??= DateTimeUzb.UZBTime;
    }
}
