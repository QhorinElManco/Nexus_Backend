namespace Nexus.Application.Dto.Response;

/// <summary>
/// Represents pagination metadata for list responses.
/// </summary>
public sealed record PaginationInfo
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public long TotalRecords { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalRecords / PageSize) : 0;
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public PaginationInfo() { }

    public PaginationInfo(int page, int pageSize, long totalRecords)
    {
        Page = page;
        PageSize = pageSize;
        TotalRecords = totalRecords;
    }
}
