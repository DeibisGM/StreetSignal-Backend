namespace StreetSignalApi.DTOs.Responses;

public class PaginationMeta
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }

    public static PaginationMeta For(int page, int pageSize, int totalItems)
    {
        var totalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);
        return new PaginationMeta
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
}
