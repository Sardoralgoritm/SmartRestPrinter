using SmartRestaurant.Domain.Models.Pagination;

namespace SmartRestaurant.Domain.Models.SortFilter;

public class SortFilterPageOptions : PageOptions, ISortFilterOptions
{
    public const string ORDER_TYPE_ASC = "ASC";
    public const string ORDER_TYPE_DESC = "DESC";

    private static readonly HashSet<string> ValidOrderTypes = new() { ORDER_TYPE_ASC, ORDER_TYPE_DESC };

    private string _orderType = ORDER_TYPE_ASC;

    public virtual string? Search { get; set; }
    public virtual string? SortBy { get; set; }

    public virtual string OrderType
    {
        get => _orderType;
        set => _orderType = ValidOrderTypes.Contains(value?.ToUpper()!) ? value?.ToUpper()! : ORDER_TYPE_ASC;
    }

    public virtual bool HasSort() => !string.IsNullOrEmpty(SortBy);
    public virtual bool HasSearch() => !string.IsNullOrEmpty(Search);
}