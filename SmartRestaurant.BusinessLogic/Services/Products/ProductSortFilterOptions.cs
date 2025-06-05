using SmartRestaurant.Domain.Models.SortFilter;

namespace SmartRestaurant.BusinessLogic.Services;

public class ProductSortFilterOptions : SortFilterPageOptions
{
    public string? Name { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? IsActive { get; set; }
}
