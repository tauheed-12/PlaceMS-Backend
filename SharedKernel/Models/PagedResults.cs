using SharedKernel.Constants;

namespace SharedKernel.Models;

/// <summary>
/// Paginated result wrapper for list endpoints.
/// All list endpoints return this — never raw List<T>.
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public static PagedResult<T> Create(List<T> items, int totalCount, int page, int pageSize)
        => new()
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

    public static PagedResult<T> Empty(int page = Pagination.DefaultPage, int pageSize = Pagination.DefaultPageSize)
        => new()
        {
            Items = new(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };
}

/// <summary>
/// Query parameters for paginated requests.
/// Inherit this in specific filter DTOs.
/// </summary>
public class PaginationQuery
{
    private int _pageSize = Pagination.DefaultPageSize;

    public int Page { get; set; } = Pagination.DefaultPage;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > Pagination.MaxPageSize
            ? Pagination.MaxPageSize
            : value < 1 ? 1 : value;
    }

    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;

    public int Skip => (Page - 1) * PageSize;
}