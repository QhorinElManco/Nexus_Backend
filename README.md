# Nexos Backend

A modern .NET backend API built with Clean Architecture and Domain-Driven Design principles.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Development Guidelines](#development-guidelines)
- [Building the Solution](#building-the-solution)
- [Running the Application](#running-the-application)
- [Docker Support](#docker-support)
- [Testing](#testing)
- [Code Style and Standards](#code-style-and-standards)
- [Configuration](#configuration)
- [Contributing](#contributing)
- [License](#license)

## Overview

Nexos Backend is a enterprise-grade .NET application following Clean Architecture principles. The solution is designed to be maintainable, testable, and scalable, with clear separation of concerns across different layers.

## Architecture

This project follows **Clean Architecture** (also known as Onion Architecture or Hexagonal Architecture) with the following layers:

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│                  (Nexos.Services.WebApi)                     │
├─────────────────────────────────────────────────────────────┤
│                   Application Layer                           │
│  ┌──────────────────┐  ┌──────────────────┐                │
│  │  Application.Dto  │  │Application.UseCases│               │
│  └──────────────────┘  └──────────────────┘                │
│  ┌──────────────────┐  ┌──────────────────┐                │
│  │Application.       │  │Application.       │                │
│  │Interface          │  │Validator          │                │
│  └──────────────────┘  └──────────────────┘                │
├─────────────────────────────────────────────────────────────┤
│                     Domain Layer                              │
│                       (Nexos.Domain)                         │
├─────────────────────────────────────────────────────────────┤
│                  Infrastructure Layer                         │
│  ┌──────────────────┐  ┌──────────────────┐                │
│  │Infrastructure    │  │Persistence       │                │
│  └──────────────────┘  └──────────────────┘                │
├─────────────────────────────────────────────────────────────┤
│                   Transversal Layer                           │
│                  (Transversal.Common)                        │
└─────────────────────────────────────────────────────────────┘
```

### Dependency Rule

Dependencies flow **inward**:
- **Presentation** depends on **Application** and **Infrastructure**
- **Application** depends on **Domain**
- **Infrastructure** depends on **Application** and **Domain**
- **Domain** has **no dependencies** (core business logic)
- **Transversal.Common** provides cross-cutting concerns

## Technology Stack

- **.NET 10.0** - Application framework
- **ASP.NET Core** - Web API framework
- **C# 12** - Programming language with latest features
- **Clean Architecture** - Architectural pattern
- **Domain-Driven Design** - Design approach
- **Docker** - Containerization
- **OpenAPI/Swagger** - API documentation

### NuGet Packages

Packages are managed centrally using [Central Package Management](https://devblogs.microsoft.com/nuget/introducing-central-package-management/):
- `Microsoft.AspNetCore.OpenApi` - OpenAPI support

## Prerequisites

- **.NET SDK 10.0** or later
- **Docker** (optional, for containerized deployment)
- **IDE**: Visual Studio 2022, JetBrains Rider, or VS Code with C# Dev Kit

### Verify Prerequisites

```bash
# Check .NET SDK version
dotnet --version

# Should output: 10.0.xxx or higher
```

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/nexos/nexos-backend.git
cd Nexos_backend
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Solution

```bash
dotnet build
```

### 4. Run the Application

```bash
cd Nexos.Services.WebApi
dotnet run
```

The API will be available at:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`

## Project Structure

```
Nexos_backend/
├── .agents/                          # AI agent skills and references
├── .github/                          # GitHub workflows and templates
├── .idea/                            # JetBrains Rider settings
├── Nexos.Domain/                     # Domain layer (entities, value objects)
├── Nexos.Application.Dto/            # Data Transfer Objects
├── Nexos.Application.Interface/      # Application service interfaces
├── Nexos.Application.UseCases/       # Use case implementations
├── Nexos.Application.Validator/      # Input validators
├── Nexos.Infrastructure/              # Infrastructure implementations
├── Nexos.Persistence/                # Data persistence layer
├── Nexos.Services.WebApi/            # API presentation layer
├── Transversal.Common/               # Shared utilities and helpers
├── Directory.Build.props             # Centralized build configuration
├── Directory.Packages.props          # Central package management
├── global.json                       # .NET SDK version pinning
├── .editorconfig                     # Code style rules
├── .gitignore                        # Git ignore patterns
├── compose.yaml                      # Docker Compose configuration
├── Nexos_backend.sln                 # Visual Studio solution file
└── README.md                         # This file
```

### Layer Responsibilities

| Layer | Responsibility | Dependencies |
|-------|---------------|--------------|
| **Domain** | Core business logic, entities, value objects | None |
| **Application.Dto** | Data transfer objects, request/response models | Domain |
| **Application.Interface** | Service interfaces, repository contracts | Domain |
| **Application.UseCases** | Business use cases, application services | Domain, Application.Interface |
| **Application.Validator** | Input validation rules | Application.Dto |
| **Infrastructure** | External services, logging, caching | Application.Interface, Domain |
| **Persistence** | Database access, repositories | Application.Interface, Domain |
| **Services.WebApi** | API controllers, middleware, configuration | All layers |
| **Transversal.Common** | Cross-cutting concerns, utilities | None |

## Development Guidelines

### Code Style

This project follows Microsoft's coding conventions with:

- **File-scoped namespaces** (C# 10+)
- **Nullable reference types** enabled
- **Implicit usings** enabled
- **Allman style** braces for classes/methods
- **4-space indentation**
- **LF line endings**

See `.editorconfig` for complete style rules.

### Naming Conventions

- **Interfaces**: Start with `I` (e.g., `IProductService`)
- **Classes**: PascalCase (e.g., `ProductService`)
- **Methods**: PascalCase (e.g., `GetProductById`)
- **Properties**: PascalCase (e.g., `ProductName`)
- **Private fields**: `_camelCase` with underscore prefix
- **Constants**: PascalCase or `UPPER_CASE`
- **Parameters**: camelCase (e.g., `productId`)

### Async/Await Pattern

```csharp
// ✅ Correct: Async all the way with CancellationToken
public async Task<Product?> GetByIdAsync(string id, CancellationToken ct = default)
{
    return await _repository.GetByIdAsync(id, ct);
}

// ❌ Wrong: Blocking on async
var result = GetByIdAsync(id).Result; // NEVER do this
```

### Dependency Injection

```csharp
// Registration in Program.cs or extension method
services.AddScoped<IProductService, ProductService>();
services.AddScoped<IProductRepository, ProductRepository>();

// Constructor injection
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    
    public ProductService(IProductRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }
}
```

### Result Pattern (Recommended)

Use Result pattern instead of throwing exceptions for business logic flow control:

```csharp
public async Task<Result<Order>> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct)
{
    // Validation
    if (request.Quantity <= 0)
        return Result<Order>.Failure("Quantity must be greater than 0", "VALIDATION_ERROR");
    
    // Business logic
    var order = await _repository.CreateAsync(request.ToEntity(), ct);
    return Result<Order>.Success(order);
}

// Usage in endpoint
app.MapPost("/orders", async (CreateOrderRequest request, IOrderService service, CancellationToken ct) =>
{
    var result = await service.CreateOrderAsync(request, ct);
    return result.IsSuccess 
        ? Results.Created($"/orders/{result.Value!.Id}", result.Value)
        : Results.BadRequest(new { error = result.Error, code = result.ErrorCode });
});
```

## Building the Solution

### Build All Projects

```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release
```

### Build Specific Project

```bash
dotnet build Nexos.Domain/Nexos.Domain.csproj
```

### Clean Build Artifacts

```bash
dotnet clean

# Or remove obj/bin folders manually
find . -type d -name "bin" -o -name "obj" | xargs rm -rf
```

## Running the Application

### Development Mode

```bash
cd Nexos.Services.WebApi
dotnet run
```

### With Hot Reload

```bash
cd Nexos.Services.WebApi
dotnet watch run
```

### With Environment Variables

```bash
# Set environment
export ASPNETCORE_ENVIRONMENT=Development

# Run with specific URL
dotnet run --urls "http://0.0.0.0:5000"
```

### Configuration Files

Configuration is loaded from:
1. `appsettings.json` - Base configuration
2. `appsettings.Development.json` - Development overrides
3. `appsettings.Production.json` - Production overrides
4. Environment variables
5. Command-line arguments

## Docker Support

### Build Docker Image

```bash
docker build -t nexos-backend:latest -f Nexos.Services.WebApi/Dockerfile .
```

### Run with Docker Compose

```bash
docker-compose up -d
```

### Docker Commands

```bash
# Build
docker-compose build

# Run in background
docker-compose up -d

# View logs
docker-compose logs -f

# Stop
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

## Testing

### Run All Tests

```bash
dotnet test
```

### Run Tests with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run Specific Test Project

```bash
dotnet test Tests/Nexos.Domain.Tests
```

### Test Structure (When Tests Are Added)

```
Tests/
├── Nexos.Domain.Tests/           # Unit tests for domain
├── Nexos.Application.Tests/      # Unit tests for application layer
├── Nexos.Infrastructure.Tests/   # Integration tests for infrastructure
└── Nexos.Api.Tests/              # API integration tests
```

### Testing Best Practices

1. **Unit Tests**: Test business logic in isolation
2. **Integration Tests**: Test system integration points
3. **Use xUnit** with FluentAssertions
4. **Achieve 80%+ code coverage**
5. **Follow AAA pattern**: Arrange, Act, Assert

```csharp
[Fact]
public async Task GetByIdAsync_WithValidId_ReturnsProduct()
{
    // Arrange
    var productId = "PROD-001";
    var expected = new Product { Id = productId, Name = "Test Product" };
    
    // Act
    var result = await _service.GetByIdAsync(productId, CancellationToken.None);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(productId, result.Id);
}
```

## Code Style and Standards

### EditorConfig

The project uses `.editorconfig` for consistent code style across IDEs:

- **Indentation**: 4 spaces
- **Line endings**: LF (Unix-style)
- **Encoding**: UTF-8
- **Braces**: Allman style (new line for classes, methods)
- **Max line length**: 120 characters

### Code Analysis

The project enables:
- **Roslyn analyzers**
- **Code style enforcement in build**
- **Treat warnings as errors**
- **Nullable reference types**

### IDE Settings

#### Visual Studio 2022

1. Enable "Enforce code style in build"
2. Enable "Run Code Analysis on build"
3. Enable "Generate .editorconfig"

#### JetBrains Rider

1. Enable "EditorConfig support"
2. Enable "ReSharper code style"
3. Enable "Solution-wide analysis"

#### VS Code

1. Install "C# Dev Kit" extension
2. Enable "EditorConfig" extension
3. Configure omnisharp analysis settings

## Configuration

### Centralized Build Configuration

`Directory.Build.props` centralizes build settings:

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

### Central Package Management

`Directory.Packages.props` manages NuGet package versions:

```xml
<Project>
    <PropertyGroup>
        <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    </PropertyGroup>
    
    <ItemGroup>
        <PackageVersion Include="Package.Name" Version="1.0.0" />
    </ItemGroup>
</Project>
```

To add a new package:
1. Add version to `Directory.Packages.props`
2. Remove version from `.csproj` files
3. Reference package in `.csproj` without version

## Contributing

### Branch Naming Convention

```
feature/description    # New features
bugfix/description      # Bug fixes
hotfix/description      # Production hotfixes
refactor/description    # Code refactoring
docs/description        # Documentation updates
```

### Commit Message Format

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
type(scope): subject

body

footer
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

Examples:
```
feat(products): add product search endpoint
fix(orders): resolve order creation validation
docs(readme): update installation instructions
refactor(domain): extract value objects to separate files
```

### Pull Request Process

1. Create feature branch from `main`
2. Implement changes following code style
3. Add/update tests
4. Ensure all tests pass
5. Update documentation
6. Create pull request with description
7. Request code review
8. Address review comments

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## Quick Reference

| Task | Command |
|------|---------|
| Restore packages | `dotnet restore` |
| Build solution | `dotnet build` |
| Run application | `dotnet run --project Nexos.Services.WebApi` |
| Run tests | `dotnet test` |
| Clean artifacts | `dotnet clean` |
| Format code | `dotnet format` |
| Docker build | `docker build -t nexos-backend .` |
| Docker compose up | `docker-compose up -d` |

## Support

For issues, questions, or suggestions:
- Create an issue in the repository
- Contact the development team

---

**Built with ❤️ using .NET 10 and Clean Architecture**