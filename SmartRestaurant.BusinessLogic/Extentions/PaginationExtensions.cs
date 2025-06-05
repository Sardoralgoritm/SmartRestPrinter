using SmartRestaurant.Domain.Models.PageResult;
using SmartRestaurant.Domain.Models.SortFilter;

namespace SmartRestaurant.BusinessLogic.Extentions;

public static class PaginationExtensions
{
    public static PagedResult<T> AsPagedResult<T>(this IQueryable<T> source, SortFilterPageOptions options)
    {
        var total = source.Count();
        var items = source.Skip((options.Page - 1) * options.PageSize)
                                .Take(options.PageSize)
                                .ToList();

        return new PagedResult<T>(items, options.Page, options.PageSize, total);
    }
}