using System.Globalization;
using Microsoft.Extensions.Options;
using Nexos.Services.WebApi.Configuration;
using NpgsqlTypes;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.PostgreSQL.ColumnWriters;

namespace Nexos.Services.WebApi.Extensions;

public static class SerilogExtensions
{
    /// <summary>
    /// Registra SerilogOptions desde la configuración y añade Serilog como proveedor de logging.
    /// </summary>
    public static IServiceCollection AddAppSerilog(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<SerilogOptions>()
            .Bind(configuration.GetSection(SerilogOptions.Section))
            .ValidateDataAnnotations();

        services.AddSerilog((serviceProvider, lc) =>
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<SerilogOptions>>()
                .Value;

            var connectionString = configuration.GetConnectionString(options.PostgreSql.ConnectionStringName);

            ConfigureLogger(lc, options, connectionString);
        });

        return services;
    }

    /// <summary>
    /// Logger de arranque usado antes de que se construya el host (detecta excepciones de arranque).
    /// Utiliza una configuración mínima solo para consola.
    /// </summary>
    public static LoggerConfiguration CreateBootstrapLogger()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static void ConfigureLogger(
        LoggerConfiguration lc,
        SerilogOptions options,
        string? connectionString)
    {
        ApplyMinimumLevels(lc, options);

        lc.Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId();

        if (options.Console.Enabled)
        {
            ApplyConsoleSink(lc, options.Console);
        }

        if (options.File.Enabled)
        {
            ApplyFileSink(lc, options.File);
        }

        if (options.PostgreSql.Enabled && !string.IsNullOrWhiteSpace(connectionString))
        {
            ApplyPostgreSqlSink(lc, options.PostgreSql, connectionString);
        }
    }

    private static void ApplyMinimumLevels(LoggerConfiguration lc, SerilogOptions options)
    {
        var defaultLevel = Enum.Parse<LogEventLevel>(options.MinimumLevel, true);
        lc.MinimumLevel.Is(defaultLevel);

        foreach (var (source, level) in options.Override)
        {
            var overrideLevel = Enum.Parse<LogEventLevel>(level, true);
            lc.MinimumLevel.Override(source, overrideLevel);
        }
    }

    private static void ApplyConsoleSink(LoggerConfiguration lc, SerilogConsoleOptions consoleOptions)
    {
        lc.WriteTo.Console(
            outputTemplate: consoleOptions.OutputTemplate,
            formatProvider: CultureInfo.InvariantCulture);
    }

    private static void ApplyFileSink(LoggerConfiguration lc, SerilogFileOptions fileOptions)
    {
        var rollingInterval = Enum.Parse<RollingInterval>(fileOptions.RollingInterval, true);

        lc.WriteTo.File(
            fileOptions.Path,
            rollingInterval: rollingInterval,
            rollOnFileSizeLimit: fileOptions.RollOnFileSizeLimit,
            fileSizeLimitBytes: fileOptions.FileSizeLimitBytes,
            retainedFileCountLimit: fileOptions.RetainedFileCountLimit,
            outputTemplate: fileOptions.OutputTemplate,
            formatProvider: CultureInfo.InvariantCulture);
    }

    private static void ApplyPostgreSqlSink(
        LoggerConfiguration lc,
        SerilogPostgreSqlOptions pgOptions,
        string connectionString)
    {
        IDictionary<string, ColumnWriterBase> columnOptions = new Dictionary<string, ColumnWriterBase>
        {
            { "id", new IdAutoIncrementColumnWriter() },
            { "timestamp", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) },
            { "level", new LevelColumnWriter(true, NpgsqlDbType.Text) },
            { "message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
            { "exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
            { "properties", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
            {
                "source_context", new SinglePropertyColumnWriter("SourceContext",
                    format: "l")
            },
            {
                "machine_name", new SinglePropertyColumnWriter("MachineName",
                    format: "l")
            },
            { "thread_id", new SinglePropertyColumnWriter("ThreadId", PropertyWriteMethod.Raw, NpgsqlDbType.Integer) }
        };

        lc.WriteTo.PostgreSQL(
            connectionString,
            pgOptions.TableName,
            columnOptions,
            needAutoCreateTable: pgOptions.NeedAutoCreateTable,
            period: pgOptions.Period,
            batchSizeLimit: pgOptions.BatchSize,
            formatProvider: CultureInfo.InvariantCulture);
    }
}
