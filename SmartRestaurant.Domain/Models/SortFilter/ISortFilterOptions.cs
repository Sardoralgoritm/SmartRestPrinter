namespace SmartRestaurant.Domain.Models.SortFilter;

public interface ISortFilterOptions
{
    string? Search { get; }

    string? SortBy { get; }

    string OrderType { get; }

    bool HasSort();

    bool HasSearch();
}
