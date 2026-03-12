namespace Nexus.Application.Dto.Response;

public sealed record ResponsePagination<T>(
    bool Success,
    string? Message,
    IReadOnlyList<T>? Data,
    PaginationInfo Pagination,
    ErrorCode ErrorCode,
    IReadOnlyList<ErrorDetail>? Errors = null
)
{
    public static ResponsePagination<T> Ok(IReadOnlyList<T> data, int page, int pageSize, long totalRecords,
        string? message = null)
    {
        return new ResponsePagination<T>(true, message, data, new PaginationInfo(page, pageSize, totalRecords),
            ErrorCode.None);
    }

    public static ResponsePagination<T> Fail(string message, ErrorCode errorCode,
        IReadOnlyList<ErrorDetail>? errors = null)
    {
        return new ResponsePagination<T>(false, message, null, new PaginationInfo(), errorCode, errors);
    }
}
