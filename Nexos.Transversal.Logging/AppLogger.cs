#pragma warning disable CA1848 // Utiliza delegados LoggerMessage para mejorar el rendimiento
#pragma warning disable CA2254 // El mensaje de la plantilla debe ser estático para el registro

using Microsoft.Extensions.Logging;

namespace Nexos.Transversal.Logging;

public class AppLogger<T>(ILogger<T> logger) : IAppLogger<T>
{
    public void LogInformation(string message, params object[] args)
    {
        logger.LogInformation(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        logger.LogWarning(message, args);
    }

    public void LogError(string message, params object[] args)
    {
        logger.LogError(message, args);
    }

    public void LogError(Exception ex, string message, params object[] args)
    {
        logger.LogError(ex, message, args);
    }

    public void LogDebug(string message, params object[] args)
    {
        logger.LogDebug(message, args);
    }
}

#pragma warning restore CA1848
#pragma warning restore CA2254
