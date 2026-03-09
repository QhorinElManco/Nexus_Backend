namespace Nexos.Transversal.Common.Response;

/// <summary>
/// Wrapper de respuesta paginada para endpoints de lista.
/// </summary>
#pragma warning disable CA1000 // Static members on generic types needed for factory pattern
public sealed record ResponsePagination<T>(
    bool Success,
    string? Message,
    T? Data,
    PaginationInfo Pagination,
    ErrorCode ErrorCode)
{
    public ResponsePagination() : this(true, null, default, new PaginationInfo(), ErrorCode.None) { }

    public ResponsePagination(T data, int page, int pageSize, long totalRecords, string? message = null)
        : this(true, message, data, new PaginationInfo(page, pageSize, totalRecords), ErrorCode.None)
    {
    }

    public static ResponsePagination<T> Ok(
        T data,
        int page,
        int pageSize,
        long totalRecords,
        string? message = null)
    {
        return new ResponsePagination<T>(true, message, data, new PaginationInfo(page, pageSize, totalRecords),
            ErrorCode.None);
    }

    public static ResponsePagination<T> Fail(string message, ErrorCode errorCode = ErrorCode.UnexpectedError)
    {
        return new ResponsePagination<T>(false, message, default, new PaginationInfo(), errorCode);
    }
}
#pragma warning restore CA1000
