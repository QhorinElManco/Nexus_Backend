namespace Nexus.Application.Dto.Response;

public enum ErrorCode
{
    None = 0,

    ValidationError = 1000,
    RequiredField,
    InvalidFormat,
    DuplicateEntry,

    NotFound = 2000,
    EntityNotFound,
    ResourceNotFound,

    Conflict = 3000,
    AlreadyExists,
    InconsistentState,

    BusinessRule = 4000,
    InsufficientStock,
    InvalidOperation,
    UnauthorizedAccess,

    ExternalService = 5000,
    ExternalServiceUnavailable,
    Timeout,

    UnexpectedError = 9999,
    Unknown
}

public static class ErrorCodeExtensions
{
    public static string ToCodeString(this ErrorCode code)
    {
        return code.ToString();
    }
}
