#pragma warning disable CA1000 // Static members on generic types needed for factory pattern
namespace Nexos.Transversal.Common.Response;

public sealed record ResponsePagination<T>(
    bool Success,
    string? Message,
    IReadOnlyList<T>? Data,
    PaginationInfo Pagination,
    ErrorCode ErrorCode)
{
    public ResponsePagination() : this(true, null, null, new PaginationInfo(), ErrorCode.None) { }

    public ResponsePagination(IReadOnlyList<T> data, int page, int pageSize, long totalRecords, string? message = null)
        : this(true, message, data, new PaginationInfo(page, pageSize, totalRecords), ErrorCode.None)
    {
    }

    public static ResponsePagination<T> Ok(IReadOnlyList<T> data, int page, int pageSize, long totalRecords,
        string? message = null)
    {
        return new ResponsePagination<T>(true, message, data, new PaginationInfo(page, pageSize, totalRecords),
            ErrorCode.None);
    }

    public static ResponsePagination<T> Fail(string message, ErrorCode errorCode = ErrorCode.UnexpectedError)
    {
        return new ResponsePagination<T>(false, message, null, new PaginationInfo(), errorCode);
    }
}
#pragma warning restore CA1000
