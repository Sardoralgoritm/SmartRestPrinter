using SmartRestaurant.Domain.Models.SortFilter;

namespace SmartRestaurant.BusinessLogic.Services.Tables;

public class TableSortFilterOption : SortFilterPageOptions
{
    public Guid? CategoryId { get; set; }
}
