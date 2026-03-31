namespace ReviewFilms.Api.DTOs.Common;

public class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = [];

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public long TotalCount { get; init; }

    public int TotalPages { get; init; }

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public static PagedResult<T> Create(
        IReadOnlyCollection<T> items,
        int pageNumber,
        int pageSize,
        long totalCount)
    {
        var safePageSize = pageSize <= 0 ? 1 : pageSize;
        var totalPages = (int)Math.Ceiling(totalCount / (double)safePageSize);

        return new PagedResult<T>
        {
            Items = items,
            PageNumber = pageNumber <= 0 ? 1 : pageNumber,
            PageSize = safePageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}
