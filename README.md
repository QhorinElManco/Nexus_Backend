# Nexus Backend

Una API backend moderna .NET construida con Arquitectura Limpia y principios de Domain-Driven Design.

## Tabla de Contenidos

- [Resumen](#resumen)
- [Arquitectura](#arquitectura)
- [Tecnologías](#tecnologías)
- [Requisitos Previos](#requisitos-previos)
- [Comenzando](#comenzando)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Guía de Desarrollo](#guía-de-desarrollo)
- [Construir la Solución](#construir-la-solución)
- [Ejecutar la Aplicación](#ejecutar-la-aplicación)
- [Soporte Docker](#soporte-docker)
- [Pruebas](#pruebas)
- [Estándares de Código](#estándares-de-código)
- [Configuración](#configuración)
- [Contribuir](#contribuir)
- [Licencia](#licencia)

## Resumen

Nexus Backend es una aplicación .NET de nivel empresarial siguiendo principios de Arquitectura Limpia. La solución está
diseñada para ser mantenible, testeable y escalable, con una clara separación de responsabilidades entre las diferentes
capas.

### Características Principales

- **Arquitectura Limpia** (Clean Architecture)
- **Entity Framework Core** con PostgreSQL
- **Migraciones automáticas** (Schema-First)
- **API REST** con OpenAPI/Swagger
- **Health Checks** integrados
- **Logging estructurado** con Serilog

## Arquitectura

Este proyecto sigue **Arquitectura Limpia** (también conocida como Arquitectura Onion o Hexagonal) con las siguientes capas:

```
┌─────────────────────────────────────────────────────────────┐
│                      Capa de Presentación                    │
│                        (Nexus.Api)                           │
├─────────────────────────────────────────────────────────────┤
│                       Capa de Aplicación                     │
│                     (Nexus.Application)                     │
├─────────────────────────────────────────────────────────────┤
│                        Capa de Dominio                       │
│                        (Nexus.Domain)                        │
├─────────────────────────────────────────────────────────────┤
│                     Capa de Infraestructura                 │
│                    (Nexus.Infrastructure)                   │
└─────────────────────────────────────────────────────────────┘
```

### Regla de Dependencias

Las dependencias fluyen **hacia adentro**:

- **Api** depende de **Application**, **Infrastructure** y **Domain**
- **Application** depende de **Domain**
- **Infrastructure** depende de **Domain**
- **Domain** no tiene dependencias (lógica de negocio core)

## Tecnologías

- **.NET 10.0** - Framework de aplicación
- **ASP.NET Core** - Framework de API web
- **C# 13** - Lenguaje de programación
- **Entity Framework Core** - ORM con PostgreSQL
- **Arquitectura Limpia** - Patrón arquitectónico
- **Domain-Driven Design** - Enfoque de diseño
- **Docker** - Contenedores
- **OpenAPI/Swagger** - Documentación de API

### Paquetes NuGet

Los paquetes se gestionan centralmente
usando [Central Package Management](https://devblogs.microsoft.com/nuget/introducing-central-package-management/):

- `Microsoft.EntityFrameworkCore` - ORM Core
- `Npgsql.EntityFrameworkCore.PostgreSQL` - Provider PostgreSQL
- `Serilog.AspNetCore` - Logging estructurado
- `Microsoft.AspNetCore.OpenApi` - Soporte OpenAPI
- `FluentValidation` - Validación de entrada

## Requisitos Previos

- **.NET SDK 10.0** o superior
- **Docker** (opcional, para despliegue en contenedores)
- **PostgreSQL** (para desarrollo local)
- **IDE**: Visual Studio 2022, JetBrains Rider, o VS Code con C# Dev Kit

### Verificar Requisitos

```bash
# Verificar versión de .NET
dotnet --version

# Debería mostrar: 10.0.xxx o superior
```

## Comenzando

### 1. Clonar el Repositorio

```bash
git clone https://github.com/QhorinElManco/Nexus_Backend.git
cd Nexu_backend
```

### 2. Restaurar Dependencias

```bash
dotnet restore
```

### 3. Construir la Solución

```bash
dotnet build
```

### 4. Configurar Base de Datos

```bash
# Aplicar migraciones
dotnet ef database update --project Nexus.Infrastructure --startup-project Nexus.Api
```

### 5. Ejecutar la Aplicación

```bash
cd Nexus.Api
dotnet run
```

La API estará disponible en:

- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`

## Estructura del Proyecto

```
Nexus_Backend/
├── .agents/                          # Habilidades y referencias de agentes IA
├── .github/                          # Flujos de trabajo de GitHub
├── .idea/                            # Configuración de JetBrains Rider
├── docs/                             # Documentación adicional
├── Nexus.Domain/                     # Capa de dominio (entidades, value objects)
├── Nexus.Application/                # Capa de aplicación (use cases, DTOs, interfaces)
├── Nexus.Infrastructure/             # Implementaciones de infraestructura
├── Nexus.Api/                        # Capa de presentación de API
├── Directory.Build.props             # Configuración centralizada de build
├── Directory.Packages.props          # Gestión central de paquetes
├── global.json                       # Versión del SDK .NET
├── .editorconfig                     # Reglas de estilo de código
├── .gitignore                        # Patrones de exclusión de Git
├── compose.yaml                      # Configuración de Docker Compose
├── Nexus_Backend.sln                 # Archivo de solución
└── README.md                         # Este archivo
```

### Responsabilidades de las Capas

| Capa               | Responsabilidad                                    | Dependencias         |
|--------------------|----------------------------------------------------|----------------------|
| **Domain**         | Lógica de negocio core, entidades                  | Ninguna              |
| **Application**    | Casos de uso, DTOs, interfaces de servicios        | Domain               |
| **Infrastructure** | Implementaciones de repositorios, servicios       | Domain, Application |
| **Api**            | Controladores API, middleware, configuración       | Todas las capas      |

## Guía de Desarrollo

### Estilo de Código

Este proyecto sigue las convenciones de Microsoft con:

- **Namespaces file-scoped** (C# 10+)
- **Nullable reference types** habilitado
- **Implicit usings** habilitado
- **Llaves Allman** para clases/métodos
- **4 espacios de indentación**
- **Saltos de línea LF**

Ver `.editorconfig` para reglas completas de estilo.

### Convenciones de Nombrado

- **Interfaces**: Inician con `I` (ej., `IProductService`)
- **Clases**: PascalCase (ej., `ProductService`)
- **Métodos**: PascalCase (ej., `GetProductById`)
- **Propiedades**: PascalCase (ej., `ProductName`)
- **Campos privados**: `_camelCase` con prefijo underscore
- **Constantes**: PascalCase o `UPPER_CASE`
- **Parámetros**: camelCase (ej., `productId`)

### Patrón Async/Await

```csharp
// ✅ Correcto: Async todo el camino con CancellationToken
public async Task<Product?> GetByIdAsync(string id, CancellationToken ct = default)
{
    return await _repository.GetByIdAsync(id, ct);
}

// ❌ Incorrecto: Bloquear en async
var result = GetByIdAsync(id).Result; // NUNCA hacer esto
```

### Inyección de Dependencias

```csharp
// Registro en Program.cs o método extensión
services.AddScoped<IProductService, ProductService>();
services.AddScoped<IProductRepository, ProductRepository>();

// Inyección en constructor
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }
}
```

### Patrón Result (Recomendado)

Usar patrón Result en lugar de lanzar excepciones para control de flujo:

```csharp
public async Task<Result<Order>> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct)
{
    // Validación
    if (request.Quantity <= 0)
        return Result<Order>.Failure("La cantidad debe ser mayor a 0", "VALIDATION_ERROR");

    // Lógica de negocio
    var order = await _repository.CreateAsync(request.ToEntity(), ct);
    return Result<Order>.Success(order);
}
```

## Construir la Solución

### Construir Todos los Proyectos

```bash
# Build debug
dotnet build

# Build release
dotnet build -c Release
```

### Construir Proyecto Específico

```bash
dotnet build Nexus.Domain/Nexus.Domain.csproj
```

### Limpiar Artefactos de Build

```bash
dotnet clean

# O remover carpetas obj/bin manualmente
find . -type d -name "bin" -o -name "obj" | xargs rm -rf
```

## Ejecutar la Aplicación

### Modo Desarrollo

```bash
cd Nexus.Api
dotnet run
```

### Con Hot Reload

```bash
cd Nexus.Api
dotnet watch run
```

### Con Variables de Entorno

```bash
# Establecer entorno
export ASPNETCORE_ENVIRONMENT=Development

# Ejecutar con URL específica
dotnet run --urls "http://0.0.0.0:5000"
```

### Archivos de Configuración

La configuración se carga desde:

1. `appsettings.json` - Configuración base
2. `appsettings.Development.json` - Sobrescrituras de desarrollo
3. `appsettings.Production.json` - Sobrescrituras de producción
4. Variables de entorno
5. Argumentos de línea de comandos

## Soporte Docker

### Construir Imagen Docker

```bash
docker build -t nexus-backend:latest -f Nexus.Api/Dockerfile .
```

### Ejecutar con Docker Compose

```bash
docker compose up -d
```

### Comandos Docker

```bash
# Construir
docker compose build

# Ejecutar en background
docker compose up -d

# Ver logs
docker compose logs -f

# Detener
docker compose down

# Detener y remover volúmenes
docker compose down -v
```

## Pruebas

### Ejecutar Todas las Pruebas

```bash
dotnet test
```

### Ejecutar Pruebas con Cobertura

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Ejecutar Proyecto de Pruebas Específico

```bash
dotnet test Tests/Nexos.Domain.Tests
```

### Estructura de Pruebas (Cuando Se Agreguen)

```
Tests/
├── Nexos.Domain.Tests/           # Pruebas unitarias de dominio
├── Nexos.Application.Tests/      # Pruebas unitarias de aplicación
├── Nexos.Infrastructure.Tests/  # Pruebas de integración de infraestructura
└── Nexos.Api.Tests/             # Pruebas de integración de API
```

### Mejores Prácticas de Pruebas

1. **Pruebas Unitarias**: Probar lógica de negocio en aislamiento
2. **Pruebas de Integración**: Probar puntos de integración del sistema
3. **Usar xUnit** con FluentAssertions
4. **Alcanzar 80%+ de cobertura de código**
5. **Seguir patrón AAA**: Arrange, Act, Assert

```csharp
[Fact]
public async Task GetByIdAsync_WithValidId_ReturnsProduct()
{
    // Arrange
    var productId = "PROD-001";
    var expected = new Product { Id = productId, Name = "Producto de Prueba" };

    // Act
    var result = await _service.GetByIdAsync(productId, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(productId, result.Id);
}
```

## Estándares de Código

### EditorConfig

El proyecto usa `.editorconfig` para estilo de código consistente:

- **Indentación**: 4 espacios
- **Saltos de línea**: LF (estilo Unix)
- **Codificación**: UTF-8
- **Llaves**: Estilo Allman (nueva línea para clases, métodos)
- **Línea máxima**: 120 caracteres

### Análisis de Código

El proyecto habilita:

- **Analizadores Roslyn**
- **Aplicación de estilo de código en build**
- **Tratar advertencias como errores**
- **Nullable reference types**

## Configuración

### Configuración Centralizada de Build

`Directory.Build.props` centraliza configuración de build:

```xml

<Project>
    <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>
        <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
        <EnableNETAnalyzers>true</EnableNETAnalyzers>
    </PropertyGroup>
</Project>
```

### Gestión Central de Paquetes

`Directory.Packages.props` gestiona versiones de paquetes NuGet:

```xml

<Project>
    <PropertyGroup>
        <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    </PropertyGroup>

    <ItemGroup>
        <PackageVersion Include="Package.Name" Version="1.0.0"/>
    </ItemGroup>
</Project>
```

Para agregar un nuevo paquete:

1. Agregar versión a `Directory.Packages.props`
2. Remover versión de archivos `.csproj`
3. Referenciar paquete en `.csproj` sin versión

### Base de Datos

La configuración de Entity Framework está documentada
en [docs/DATABASE_CONFIGURATION.md](docs/DATABASE_CONFIGURATION.md).

#### Migraciones

```bash
# Crear nueva migración
dotnet ef migrations add NombreMigracion \
    --project Nexus.Infrastructure \
    --startup-project Nexus.Api

# Aplicar migraciones
dotnet ef database update \
    --project Nexus.Infrastructure \
    --startup-project Nexus.Api

# Generar script SQL
dotnet ef migrations script \
    --project Nexus.Infrastructure \
    --startup-project Nexus.Api \
    -o migration.sql
```

## Contribuir

### Convención de Nombrado de Ramas

```
feature/descripcion    # Nuevas características
bugfix/descripcion     # Correcciones de bugs
hotfix/descripcion     # Hotfixes de producción
refactor/descripcion   # Refactorización de código
docs/descripcion       # Actualizaciones de documentación
```

### Formato de Mensajes de Commit

Seguir [Conventional Commits](https://www.conventionalcommits.org/):

```
tipo(alcance): asunto

cuerpo

pie
```

Tipos: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

Ejemplos:

```
feat(productos): agregar endpoint de búsqueda de productos
fix(pedidos): resolver validación de creación de pedidos
docs(readme): actualizar instrucciones de instalación
refactor(dominio): extraer value objects a archivos separados
```

### Proceso de Pull Request

1. Crear rama de característica desde `main`
2. Implementar cambios siguiendo estilo de código
3. Agregar/actualizar pruebas
4. Asegurar que todas las pruebas pasen
5. Actualizar documentación
6. Crear pull request con descripción
7. Solicitar code review
8. Atender comentarios del review

## Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para detalles.

---

## Referencia Rápida

| Tarea               | Comando                                      |
|---------------------|----------------------------------------------|
| Restaurar paquetes  | `dotnet restore`                             |
| Construir solución  | `dotnet build`                               |
| Ejecutar aplicación | `dotnet run --project Nexus.Api`            |
| Ejecutar pruebas    | `dotnet test`                                |
| Limpiar artefactos  | `dotnet clean`                               |
| Formatear código    | `dotnet format`                              |
| Construir docker    | `docker build -t nexus-backend .`           |
| Docker compose up   | `docker compose up -d`                       |
| Aplicar migraciones | `dotnet ef database update`                 |

---

**Construido con ❤️ usando .NET 10 y Arquitectura Limpia**
