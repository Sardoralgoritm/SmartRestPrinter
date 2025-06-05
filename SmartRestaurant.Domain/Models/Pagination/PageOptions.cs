namespace SmartRestaurant.Domain.Models.Pagination;

public class PageOptions : IPageOptions
{
    public const int DEFAULT_PAGE_SIZE = 20;
    public const int DEFAULT_PAGE_SIZE_LIMIT = 1000;

    private int _page = 1;
    private int _pageSize = DEFAULT_PAGE_SIZE;

    public virtual int Page
    {
        get => _page;
        set => _page = value > 0 ? value : 1;
    }

    public virtual int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 0 && value <= DEFAULT_PAGE_SIZE_LIMIT ? value : DEFAULT_PAGE_SIZE;
    }
}