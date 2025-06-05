using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.BusinessLogic.Services.Tables.QueryObjects;

public static class TableViewListSortFilter
{
    public static IQueryable<Table> SortFilter(this IQueryable<Table> tables, TableSortFilterOption filter)
    {
        if (filter.CategoryId.HasValue)
        {
            tables = tables.Where(bd => bd.TableCategoryId == filter.CategoryId);
        }

        return tables;
    }
}
