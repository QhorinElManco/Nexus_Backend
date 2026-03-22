# Logging en Nexus.Infrastructure

Este documento describe la implementación de logging en `Nexus.Infrastructure` usando Serilog.

## Visión general

La configuración de logging es responsabilidad de la capa de infraestructura. El punto de entrada público es
`DependencyInjection.AddInfrastructureServices(...)`, que internamente invoca los helpers específicos de logging y
persistencia.

## Estructura relevante

```
Nexus.Infrastructure/
├── DependencyInjection.cs        ← Punto de entrada público (AddInfrastructureServices)
├── LoggingExtensions.cs          ← Configuración de Serilog (internal)
├── PersistenceExtensions.cs      ← Configuración de persistencia (internal)
├── Logging/
│   └── SerilogOptions.cs        ← opciones de configuración
└── Persistence/
    └── NexusDbContext.cs
```

## Qué hace LoggingExtensions

- Provee `AddTransversalLoggingServices(IServiceCollection, IConfiguration)` (internal)
- Registra `IOptions<SerilogOptions>` desde la sección `Serilog` en `appsettings.json`
- Configura Serilog con sinks configurables: Console, File y PostgreSQL

Las implementaciones internas y helpers (por ejemplo creación de columnas PostgreSQL, formateo, niveles) están
encapsuladas en `LoggingExtensions.cs`.

## Opciones (SerilogOptions)

Contiene las clases de configuración que se leen desde `appsettings.json`:

- `SerilogOptions` (raiz)
- `SerilogConsoleOptions`
- `SerilogFileOptions`
- `SerilogPostgreSqlOptions`

Ejemplo (resumido) en `appsettings.json`:

```json
{
    "Serilog": {
        "MinimumLevel": "Information",
        "Override": {
            "Microsoft": "Warning",
            "System": "Warning"
        },
        "Console": {
            "Enabled": true
        },
        "File": {
            "Enabled": true,
            "Path": "logs/log-.txt"
        },
        "PostgreSql": {
            "Enabled": true,
            "ConnectionStringName": "DefaultConnection",
            "TableName": "logs"
        }
    }
}
```

## Uso en Program.cs (patrón actual)

1. Crear un *bootstrap logger* antes de configurar el host (captura errores tempranos):

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();
```

2. En el composition root (API):

```csharp
var builder = WebApplication.CreateBuilder(args);

// Infrastructure: persistence + logging
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);

// Application: casos de uso y validadores
builder.Services.AddApplicationUseCasesServices();

var app = builder.Build();
```

El bootstrap logger se mantiene para capturar fallos antes de que Serilog sea configurado completamente por DI.

## Uso en servicios

Los servicios utilizan `ILogger<T>` de `Microsoft.Extensions.Logging.Abstractions` directamente:

```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;
    public MyService(ILogger<MyService> logger) => _logger = logger;

    public void Do() => _logger.LogInformation("Doing work");
}
```

## Ventajas

1. Separation of concerns: logging queda en Infrastructure
2. Reutilización: cualquier proyecto puede llamar `AddInfrastructureServices(...)`
3. Configuración centralizada y controlada por `appsettings.json`
4. Soporte para logging estructurado (PostgreSQL) y sinks múltiples
5. Bootstrap logger para errores de arranque

## Notas importantes

- `LoggingExtensions.cs` es `internal` — `DependencyInjection.cs` expone la superficie pública de la capa.
- La tabla de logs en PostgreSQL puede crearse automáticamente si `NeedAutoCreateTable` está activo.
- Ajusta `appsettings.json` según tu entorno y necesidades de retención/rotación.
- Los warnings CA1848 y CA1873 están suprimidos globalmente en `Directory.Build.props` para simplificar el uso de logging.
