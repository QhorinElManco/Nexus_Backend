# Patrones C# Modernos

## File-Scoped Namespaces y Primary Constructors

```csharp
namespace MyApp.Domain;

// Primary constructor (C# 12)
public class ProductService(IProductRepository repository, ILogger<ProductService> logger)
{
    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        logger.LogInformation("Fetching product {ProductId}", id);
        return await repository.GetByIdAsync(id, ct);
    }
}

// Record con primary constructor
public record Product(int Id, string Name, decimal Price)
{
    public bool IsExpensive => Price > 100m;
}
```

## Tipos Record y Pattern Matching

```csharp
// Record inmutable
public record Customer(int Id, string Name, string Email);

// Record con validación
public record OrderRequest(int ProductId, int Quantity)
{
    public OrderRequest : this(ProductId, Quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(Quantity);
    }
}

// Pattern matching con records
public decimal CalculateDiscount(Customer customer, Order order) => customer switch
{
    { Id: > 1000 } => order.Total * 0.2m,          // Cliente premium
    { Name: "VIP" } => order.Total * 0.3m,          // VIP
    _ when order.Total > 500 => order.Total * 0.1m, // Pedido grande
    _ => 0m
};

// List patterns (C# 11+)
public string DescribeItems(int[] items) => items switch
{
    [] => "Empty",
    [var single] => $"One item: {single}",
    [var first, .., var last] => $"Multiple items from {first} to {last}",
    _ => "Unknown"
};
```

## Tipos Nullable Reference

```csharp
#nullable enable

public class UserService
{
    // Parámetro y tipo de retorno no nullable
    public User CreateUser(string email, string name)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(name);

        return new User { Email = email, Name = name };
    }

    // Tipo de retorno nullable
    public User? FindUserByEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return _repository.Find(email);
    }

    // Required modifier (C# 11)
    public class User
    {
        public required string Email { get; init; }
        public required string Name { get; init; }
        public string? PhoneNumber { get; init; } // Opcional
    }
}

// Null-forgiving operator (usar con moderación)
var user = FindUserById(id)!; // Solo si estás seguro

// Null-coalescing assignment
_cache ??= new Dictionary<string, object>();
```

## Patrones de Colecciones Modernos

```csharp
// Collection expressions (C# 12)
int[] numbers = [1, 2, 3, 4, 5];
List<string> names = ["Alice", "Bob", "Charlie"];

// Operador spread
int[] moreNumbers = [..numbers, 6, 7, 8];
string[] allNames = [..names, "David"];

// ReadOnly collections
public IReadOnlyList<Product> Products { get; } = [product1, product2];

// Frozen collections para rendimiento
using System.Collections.Frozen;

private static readonly FrozenDictionary<string, int> StatusCodes =
    new Dictionary<string, int>
    {
        ["Active"] = 1,
        ["Inactive"] = 2,
        ["Pending"] = 3
    }.ToFrozenDictionary();
```

## Expression-Bodied Members

```csharp
public class Product
{
    private decimal _price;

    // Expression-bodied property
    public decimal Price
    {
        get => _price;
        init => _price = value > 0 ? value : throw new ArgumentException();
    }

    // Expression-bodied method
    public decimal GetPriceWithTax(decimal taxRate) => _price * (1 + taxRate);

    // Expression-bodied constructor (con validación)
    public Product(string name) => Name = !string.IsNullOrWhiteSpace(name)
        ? name
        : throw new ArgumentException(nameof(name));

    public required string Name { get; init; }
}
```

## String Interpolation y Raw Strings

```csharp
// Raw string literals (C# 11)
var json = """
    {
        "name": "Product",
        "price": 99.99,
        "available": true
    }
    """;

// Interpolated raw strings
var productJson = $$
    {
        "id": {{product.Id}},
        "name": "{{product.Name}}",
        "price": {{product.Price}}
    }
    """;

// UTF-8 string literals
ReadOnlySpan<byte> utf8 = "Hello"u8;
```

## Global Using Directives

```csharp
// GlobalUsings.cs
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.DependencyInjection;
```

## Source Generators (Preparación)

```csharp
// Usar partial classes para source generators
public partial class UserRepository
{
    // El generador agregará métodos aquí
}

// Ejemplo: JsonSerializer source generation
using System.Text.Json.Serialization;

[JsonSerializable(typeof(Product))]
[JsonSerializable(typeof(List<Product>))]
internal partial class AppJsonContext : JsonSerializerContext
{
}

// Uso
var json = JsonSerializer.Serialize(product, AppJsonContext.Default.Product);
```

## Discriminated Unions con Records

```csharp
// Base record para patrón result
public abstract record Result<T>
{
    public record Success(T Value) : Result<T>;
    public record Failure(string Error) : Result<T>;
}

// Uso
public Result<User> GetUser(int id) =>
    _repository.Find(id) is User user
        ? new Result<User>.Success(user)
        : new Result<User>.Failure("User not found");

// Pattern matching en result
var message = GetUser(id) switch
{
    Result<User>.Success(var user) => $"Found: {user.Name}",
    Result<User>.Failure(var error) => $"Error: {error}",
    _ => "Unknown"
};
```

## Referencia Rápida

| Feature | Versión C# | Ejemplo |
|---------|------------|---------|
| File-scoped namespace | C# 10 | `namespace MyApp;` |
| Primary constructors | C# 12 | `class Service(ILogger logger)` |
| Required members | C# 11 | `public required string Name { get; init; }` |
| Raw string literals | C# 11 | `var s = """ multi-line """;` |
| List patterns | C# 11 | `[1, 2, .., var last]` |
| Collection expressions | C# 12 | `int[] x = [1, 2, 3];` |
| Init-only properties | C# 9 | `public string Name { get; init; }` |
| Record types | C# 9 | `record Person(string Name);` |
