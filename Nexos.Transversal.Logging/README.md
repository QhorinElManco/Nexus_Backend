# Adaptación de SerilogExtensions a ConfigureServices

## Cambios Realizados

### 1. Creación de SerilogOptions.cs

Se movieron las clases de configuración desde `Nexos.Services.WebApi.Configuration` al proyecto
`Nexos.Transversal.Logging`:

- `SerilogOptions`: Configuración raíz para Serilog
- `SerilogConsoleOptions`: Configuración para el sink de consola
- `SerilogFileOptions`: Configuración para el sink de archivos
- `SerilogPostgreSqlOptions`: Configuración para el sink de PostgreSQL

### 2. Actualización de ConfigureServices.cs

Se adaptó completamente el código del método de extensión `AddAppSerilog` al método `AddTransversalServices`:

#### Características principales:

- **Registro de opciones**: Utiliza el patrón de Options Pattern para configurar Serilog desde `appsettings.json`
- **Bootstrap Logger**: Método `CreateBootstrapLogger()` para logging durante el arranque de la aplicación
- **Múltiples sinks soportados**:
    - Console (consola)
    - File (archivos rodantes)
    - PostgreSQL (base de datos)

#### Configuración de PostgreSQL:

- Columnas automáticas: id, timestamp, level, message, exception, properties
- Propiedades adicionales: source_context, machine_name, thread_id
- Auto-creación de tabla opcional
- Escritura en lotes (batch)

### 3. Estructura de Configuración (appsettings.json)

```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "Override": {
      "Microsoft": "Warning",
      "System": "Warning"
    },
    "Console": {
      "Enabled": true,
      "OutputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"
    },
    "File": {
      "Enabled": true,
      "Path": "logs/log-.txt",
      "RollingInterval": "Day",
      "RollOnFileSizeLimit": true,
      "FileSizeLimitBytes": 10485760,
      "RetainedFileCountLimit": 30,
      "OutputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"
    },
    "PostgreSql": {
      "Enabled": true,
      "ConnectionStringName": "DefaultConnection",
      "TableName": "logs",
      "NeedAutoCreateTable": true,
      "BatchSize": 100,
      "Period": "00:00:05"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=nexos;Username=postgres;Password=password"
  }
}
```

### 4. Uso en Program.cs

```csharp
// Logger de arranque (antes de construir el host)
Log.Logger = ConfigureServices.CreateBootstrapLogger().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Agregar servicios transversales con Serilog
    builder.Services.AddTransversalServices(builder.Configuration);

    var app = builder.Build();

    // ... configuración de la app

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación no pudo iniciar");
}
finally
{
    Log.CloseAndFlush();
}
```

## Ventajas de esta Implementación

1. **Centralización**: Toda la configuración de logging está en un solo proyecto transversal
2. **Reutilización**: Cualquier proyecto puede usar `AddTransversalServices()`
3. **Configuración flexible**: Los sinks se pueden habilitar/deshabilitar desde `appsettings.json`
4. **Logging estructurado**: Soporte para PostgreSQL con columnas tipadas
5. **Bootstrap logging**: Captura errores de arranque de la aplicación
6. **Enriquecimiento**: Automáticamente agrega información de contexto, máquina y thread

## Notas Importantes

- El proyecto ya tiene todas las dependencias necesarias de NuGet instaladas
- La tabla de logs se creará automáticamente en PostgreSQL si `NeedAutoCreateTable` está en `true`
- Los archivos de log se rotan automáticamente según el `RollingInterval` configurado
- Los niveles de log se pueden sobrescribir por namespace usando `Override`

