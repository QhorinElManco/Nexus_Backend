namespace Nexos.Transversal.Logging;

/// <summary>
/// Root options for Serilog configuration bound from the "Serilog" section in appsettings.json.
/// </summary>
public sealed class SerilogOptions
{
    public const string Section = "Serilog";

    public string MinimumLevel { get; set; } = "Information";
    public Dictionary<string, string> Override { get; set; } = [];
    public SerilogConsoleOptions Console { get; set; } = new();
    public SerilogFileOptions File { get; set; } = new();
    public SerilogPostgreSqlOptions PostgreSql { get; set; } = new();
}

/// <summary>
/// Options for the console sink.
/// </summary>
public sealed class SerilogConsoleOptions
{
    public bool Enabled { get; set; } = true;

    public string OutputTemplate { get; set; } =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}";
}

/// <summary>
/// Options for the rolling file sink.
/// </summary>
public sealed class SerilogFileOptions
{
    public bool Enabled { get; set; } = true;
    public string Path { get; set; } = "logs/log-.txt";
    public string RollingInterval { get; set; } = "Day";
    public bool RollOnFileSizeLimit { get; set; } = true;
    public long FileSizeLimitBytes { get; set; } = 10_485_760; // 10 MB
    public int RetainedFileCountLimit { get; set; } = 30;

    public string OutputTemplate { get; set; } =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}";
}

/// <summary>
/// Options for the PostgreSQL sink.
/// Uses the ConnectionStrings section to avoid duplicating the connection string.
/// </summary>
public sealed class SerilogPostgreSqlOptions
{
    public bool Enabled { get; set; }
    public string ConnectionStringName { get; set; } = "DefaultConnection";
    public string TableName { get; set; } = "Logs";
    public bool NeedAutoCreateTable { get; set; } = true;
    public int BatchSize { get; set; } = 100;
    public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(5);
}
