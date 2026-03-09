namespace Nexos.Transversal.Common.Response;

/// <summary>
/// Generic response wrapper for API operations.
/// </summary>
#pragma warning disable CA1000 // Static members on generic types needed for factory pattern
public sealed record Response<T>(bool Success, string? Message, T? Data, ErrorCode ErrorCode)
{
    public Response() : this(true, null, default, ErrorCode.None) { }

    public Response(T data, string? message = null)
        : this(true, message, data, ErrorCode.None)
    {
    }

    public static Response<T> Ok(T data, string? message = null)
    {
        return new Response<T>(true, message, data, ErrorCode.None);
    }

    public static Response<T> Fail(string message, ErrorCode errorCode = ErrorCode.UnexpectedError)
    {
        return new Response<T>(false, message, default, errorCode);
    }
}
#pragma warning restore CA1000
