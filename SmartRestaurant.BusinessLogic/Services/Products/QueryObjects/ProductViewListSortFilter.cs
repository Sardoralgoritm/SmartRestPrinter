using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services;

public static class ProductViewListSortFilter
{
    public static IQueryable<Product> SortFilter(this IQueryable<Product> products, ProductSortFilterOptions filter)
    {
        if (filter.CategoryId.HasValue)
        {
            products = products.Where(bd => bd.CategoryId == filter.CategoryId);
        }

        if (string.IsNullOrEmpty(filter.SortBy))
        {
            products = products.OrderByDescending(bd => bd.Id);
        }

        if (filter.IsActive.HasValue)
        {
            products = products.Where(bd => bd.IsActive == filter.IsActive);
        }

        if (filter.Name != null)
        {
            products = products.Where(bd => bd.Name.ToLower().StartsWith(filter.Name.ToLower()));
        }

        return products;
    }
}