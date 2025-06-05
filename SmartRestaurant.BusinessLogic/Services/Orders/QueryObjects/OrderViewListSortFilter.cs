using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services;

public static class OrderViewListSortFilter
{
    public static IQueryable<Order> SortFilter(this IQueryable<Order> orders, OrderSortFilterOptions filter)
    {
        if (filter.FromDateTime != null)
        {
            orders = orders.Where(o => o.CreatedAt.Date >= filter.FromDateTime.Value.Date);
        }

        if (filter.ToDateTime != null)
        {
            orders = orders.Where(o => o.CreatedAt.Date <= filter.ToDateTime.Value.Date);
        }

        if (!string.IsNullOrEmpty(filter.Status))
        {
            orders = orders.Where(o => o.Status == filter.Status);
        }

        if (!string.IsNullOrEmpty(filter.SortBy))
        {
            orders = filter.SortBy switch
            {
                "queue" => orders.OrderBy(o => o.QueueNumber),
                "createdAt" => orders.OrderBy(o => o.CreatedAt),
                _ => orders
            };
        }

        if (filter.OrderedUserId != null)
        {
            orders = orders.Where(o => o.OrderedByUserId == filter.OrderedUserId);
        }

        if (filter.ClosedUserId != null)
        {
            orders = orders.Where(o => o.ClosedByUserId == filter.ClosedUserId);
        }

        return orders;
    }
}