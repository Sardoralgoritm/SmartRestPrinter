namespace SmartRestaurant.Domain.Models.PageResult;

public class PagedResult<T> : IPagedResult
{
    public int Page { get; }
    public int PageSize { get; }
    public long Total { get; }
    public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
    public List<T> Items { get; }

    public PagedResult(List<T> items, int page, int pageSize, long total)
    {
        Items = items ?? new List<T>();
        Page = page;
        PageSize = pageSize;
        Total = total;
    }
}