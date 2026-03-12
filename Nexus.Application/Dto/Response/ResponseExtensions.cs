using FluentValidation.Results;

namespace Nexus.Application.Dto.Response;

/// <summary>
/// Proporciona métodos de extensión para transformar instancias de FluentValidation.ValidationResult en los tipos
/// de respuesta estructurada utilizados dentro de la aplicación.
/// </summary>
public static class ResponseExtensions
{
    /// <summary>
    /// Convierte un ValidationResult en Response{T} con errores estructurados.
    /// </summary>
    public static Response<T> ToFailureResponse<T>(
        this ValidationResult validationResult,
        ErrorCode errorCode = ErrorCode.ValidationError,
        string? errorMessage = null)
    {
        var errorDetails = validationResult.Errors
            .Select(f => new ErrorDetail(
                Property: f.PropertyName,
                Message: f.ErrorMessage))
            .ToList();

        errorMessage ??= "Validation failed";

        return Response<T>.Fail(errorMessage, errorCode, errorDetails);
    }

    /// <summary>
    /// Convierte un ValidationResult en ResponsePagination{T} (útil para endpoints de búsqueda).
    /// </summary>
    public static ResponsePagination<T> ToFailureResponsePagination<T>(
        this ValidationResult validationResult,
        ErrorCode errorCode = ErrorCode.ValidationError,
        string? errorMessage = null)
    {
        var errorDetails = validationResult.Errors
            .Select(f => new ErrorDetail(
                Property: f.PropertyName,
                Message: f.ErrorMessage))
            .ToList();

        errorMessage ??= "Validation failed";

        return ResponsePagination<T>.Fail(errorMessage, errorCode, errorDetails);
    }
}
