# Nexus.Tests - Estructura de Tests

## Organización

Los tests están organizados por **módulo/dominio del negocio**, siguiendo la estructura de carpetas de la capa
Application.

```
Nexus.Tests/
├── Auth/                    → Tests de autenticación y autorización
├── Companies/               → Tests de Companies
├── Customers/              → Tests de Customers
├── Suppliers/              → Tests de Suppliers
├── Roles/                   → Tests de Roles
├── Access/                  → Tests de Access
├── Categories/              → Tests de Categories
├── WarehouseTypes/          → Tests de WarehouseTypes
├── Users/                   → Tests de Users
├── Products/                → Tests de productos e inventario
│   ├── Repositories/        → Tests de repositorios
│   └── Services/            → Tests de servicios
├── Sales/                   → Tests de ventas
├── Middleware/              → Tests de middleware
└── Shared/                  → Helpers, builders, fixtures
```

## Naming Convention

```
{Servicio}{Tipo}Tests.cs

Ejemplos:
├── OrderServiceTests.cs
├── DeliveryServiceTests.cs
├── KardexEntryServiceTests.cs
└── ClaimsExtractorTests.cs
```

## Tipos de Test

| Tipo            | Descripción                                  | Cuándo usar               |
|-----------------|----------------------------------------------|---------------------------|
| **Unit**        | Testea una unidad aislada (servicio, método) | Mayoría de los casos      |
| **Integration** | Testea interacción entre componentes         | Repositorios, controllers |
| **E2E**         | Testea flujos completos                      | Casos de uso críticos     |

## Run Tests

```bash
# Todos los tests
dotnet test

# Tests de un dominio específico
dotnet test --filter "FullyQualifiedName~Sales"

# Con coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Agregar nuevos tests

1. Elegir la carpeta según el dominio del servicio
2. Nombre del archivo: `{NombreDelServicio}Tests.cs`
3. Heredar de fixtures compartidas en `Shared/` si es necesario

## Fixtures y Helpers disponibles

- `TestClaimsPrincipalBuilder` - Para crear Claims de test
- Domain builders - Para crear entidades de test
