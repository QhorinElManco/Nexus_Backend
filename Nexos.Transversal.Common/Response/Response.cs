namespace Nexos.Transversal.Common.Response;

/// <summary>
/// Generic response wrapper for API operations.
/// </summary>
public sealed record Response<T>(
    bool Success,
    string? Message,
    T? Data,
    ErrorCode ErrorCode,
    IReadOnlyList<ErrorDetail>? Errors = null)
{
    public static Response<T> Ok(T data, string? message = null)
    {
        return new Response<T>(true, message, data, ErrorCode.None);
    }

    public static Response<T> Fail(string message, ErrorCode errorCode, IReadOnlyList<ErrorDetail>? errors = null)
    {
        return new Response<T>(false, message, default, errorCode, errors);
    }
}
