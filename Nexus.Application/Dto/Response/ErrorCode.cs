namespace Nexus.Application.Dto.Response;

public enum ErrorCode
{
    None = 0,

    ValidationError = 400,
    NotFound = 404,
    Conflict = 409,
    Unauthorized = 401,
    Forbidden = 403,
    BusinessRule = 422,
    UnexpectedError = 500
}

public static class ErrorCodeExtensions
{
    public static string ToCodeString(this ErrorCode code)
    {
        return code.ToString();
    }

    public static int ToHttpStatusCode(this ErrorCode code)
    {
        return (int)code;
    }
}
