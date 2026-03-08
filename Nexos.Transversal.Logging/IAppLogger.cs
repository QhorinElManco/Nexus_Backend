namespace Nexos.Transversal.Logging;

public interface IAppLogger<T>
{
    public void LogInformation(string message, params object[] args);
    public void LogWarning(string message, params object[] args);
    public void LogError(string message, params object[] args);
    public void LogError(Exception ex, string message, params object[] args);
    public void LogDebug(string message, params object[] args);
}
