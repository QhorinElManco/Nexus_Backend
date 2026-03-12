namespace Nexos.Transversal.Common.Response;

/// <summary>
/// Detalle estructurado de errores para respuestas (por ejemplo, errores de validación).
/// </summary>
public sealed record ErrorDetail(
    string? Property,
    string Message
);
