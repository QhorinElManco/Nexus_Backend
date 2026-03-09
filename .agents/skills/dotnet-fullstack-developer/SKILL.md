---
name: dotnet-fullstack-developer
description: "Utilizar al construir aplicaciones backend con C#/.NET 8+, ASP.NET Core APIs o Blazor. Proporciona patrones de Clean Architecture, EF Core, Dapper, caching, testing y mejores prácticas empresariales."
---

# Desarrollador Fullstack .NET

Desarrollador senior C# que construye aplicaciones robustas, escalables y listas para producción.

## Cuándo Usar Esta Habilidad

- ASP.NET Core APIs (Minimal APIs o Controladores)
- Clean Architecture con Domain-Driven Design
- Acceso a datos con EF Core o Dapper
- Caching con Redis/Memory Cache
- Autenticación y autorización
- Pruebas unitarias y de integración con xUnit

## Estructura del Proyecto

```
src/
├── Domain/              # Entidades, interfaces, value objects
├── Application/         # Servicios, DTOs, validación
├── Infrastructure/     # EF Core, Dapper, Redis, DI
└── Api/                # Controllers, Minimal APIs, Program.cs
```

## Reglas Obligatorias

### ✅ DEBE HACER
- Usar nullable reference types
- Usar file-scoped namespaces y primary constructors (C# 12)
- Aplicar async/await para I/O
- Usar inyección de dependencias
- Incluir CancellationToken en métodos async
- Usar IOptions<T> para configuración
- Implementar patrón Result para manejo de errores

### ❌ NO DEBE HACER
- Usar .Result o .Wait() en código async
- Exponer entidades EF Core en respuestas API
- Crear HttpClient manualmente (usar IHttpClientFactory)
- Omitir validación de entrada

## Acceso a Datos

| Tecnología | Cuándo Usar |
|------------|-------------|
| EF Core | Modelos complejos, relaciones, migraciones |
| Dapper | Queries de alto rendimiento, lectura intensiva |

Ver `references/ef-core.md` para EF Core.
Ver `references/dapper.md` para Dapper.

## Guías de Referencia

| Tema | Archivo |
|------|---------|
| C# Moderno | `references/modern-csharp.md` |
| ASP.NET Core | `references/aspnet-core.md` |
| Entity Framework | `references/ef-core.md` |
| Dapper | `references/dapper.md` |
| Rendimiento | `references/performance.md` |
| Blazor | `references/blazor.md` |

## Plantillas

- `assets/service-template.cs` - Implementación de servicios
- `assets/repository-template.cs` - Repositorios

## Stack Tecnológico

.NET 8, ASP.NET Core, Minimal APIs, Blazor, EF Core, Dapper, xUnit, Moq, FluentValidation, Serilog, Redis, Clean Architecture, CQRS
