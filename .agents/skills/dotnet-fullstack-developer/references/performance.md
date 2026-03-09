# Optimización de Rendimiento

## Span<T> y Memory<T>

```csharp
// Manipulación tradicional de strings (asigna memoria)
public string ProcessStringOld(string input)
{
    return input.Substring(0, 10).ToUpper();
}

// Usando Span<T> (sin asignación)
public string ProcessStringNew(ReadOnlySpan<char> input)
{
    Span<char> buffer = stackalloc char[10];
    input[..10].ToUpperInvariant(buffer);
    return new string(buffer);
}

// Parsing con Span<T>
public int ParseNumber(ReadOnlySpan<char> text)
{
    return int.Parse(text);
}

// Stack allocation para arrays pequeños
public void ProcessSmallArray()
{
    Span<int> numbers = stackalloc int[10];
    for (int i = 0; i < numbers.Length; i++)
    {
        numbers[i] = i * 2;
    }
}

// Trabajando con datos byte
public void ProcessBytes(ReadOnlySpan<byte> data)
{
    // Acceso directo a memoria, sin asignaciones
    for (int i = 0; i < data.Length; i++)
    {
        var byte = data[i];
        // Process byte
    }
}
```

## ArrayPool para Reutilización de Buffers

```csharp
using System.Buffers;

public class BufferProcessor
{
    public async Task ProcessLargeDataAsync(Stream stream, CancellationToken ct)
    {
        // Alquilar array del pool
        var buffer = ArrayPool<byte>.Shared.Rent(4096);

        try
        {
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, ct)) > 0)
            {
                // Procesar buffer[0..bytesRead]
                ProcessChunk(buffer.AsSpan(0, bytesRead));
            }
        }
        finally
        {
            // Siempre devolver al pool
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private void ProcessChunk(ReadOnlySpan<byte> chunk)
    {
        // Lógica de procesamiento
    }
}
```

## Mejores Prácticas Async

```csharp
// Usar ValueTask para rutas frecuentemente síncronas
public class CacheService
{
    private readonly Dictionary<string, string> _cache = new();

    public ValueTask<string?> GetAsync(string key)
    {
        // Si está en cache, retornar sincrónicamente sin asignación
        if (_cache.TryGetValue(key, out var value))
            return ValueTask.FromResult<string?>(value);

        // De lo contrario, camino async
        return LoadFromDatabaseAsync(key);
    }

    private async ValueTask<string?> LoadFromDatabaseAsync(string key)
    {
        var value = await _database.GetAsync(key);
        _cache[key] = value;
        return value;
    }
}

// ConfigureAwait(false) en librerías
public async Task<Data> GetDataAsync()
{
    var response = await _httpClient.GetAsync("api/data")
        .ConfigureAwait(false);
    return await response.Content.ReadFromJsonAsync<Data>()
        .ConfigureAwait(false);
}

// Evitar async void excepto para event handlers
public async void ButtonClick(object sender, EventArgs e) // OK para eventos
{
    try
    {
        await ProcessClickAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing click");
    }
}

// Soporte de cancellation token
public async Task<List<Product>> GetProductsAsync(CancellationToken ct = default)
{
    return await _dbContext.Products
        .AsNoTracking()
        .ToListAsync(ct);
}

// Operaciones async paralelas
public async Task<(User user, Orders orders, Profile profile)> GetUserDataAsync(int userId)
{
    var userTask = _userService.GetAsync(userId);
    var ordersTask = _orderService.GetByUserAsync(userId);
    var profileTask = _profileService.GetAsync(userId);

    await Task.WhenAll(userTask, ordersTask, profileTask);

    return (await userTask, await ordersTask, await profileTask);
}
```

## Object Pooling

```csharp
using Microsoft.Extensions.ObjectPool;

// Definir política de objeto pooled
public class StringBuilderPooledObjectPolicy : PooledObjectPolicy<StringBuilder>
{
    public override StringBuilder Create() => new StringBuilder();

    public override bool Return(StringBuilder obj)
    {
        obj.Clear();
        return obj.Capacity <= 4096; // No pooling si es muy grande
    }
}

// Registrar en DI
builder.Services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
builder.Services.AddSingleton(serviceProvider =>
{
    var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
    return provider.Create(new StringBuilderPooledObjectPolicy());
});

// Uso
public class MessageFormatter(ObjectPool<StringBuilder> pool)
{
    public string FormatMessage(string template, params object[] args)
    {
        var builder = pool.Get();
        try
        {
            builder.AppendFormat(template, args);
            return builder.ToString();
        }
        finally
        {
            pool.Return(builder);
        }
    }
}
```

## Benchmarking con BenchmarkDotNet

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class StringBenchmarks
{
    private const string Input = "Hello, World!";

    [Benchmark(Baseline = true)]
    public string UsingSubstring()
    {
        return Input.Substring(0, 5).ToUpper();
    }

    [Benchmark]
    public string UsingSpan()
    {
        ReadOnlySpan<char> span = Input.AsSpan(0, 5);
        return span.ToString().ToUpper();
    }

    [Benchmark]
    public string UsingSpanWithStackAlloc()
    {
        ReadOnlySpan<char> input = Input;
        Span<char> buffer = stackalloc char[5];
        input[..5].ToUpperInvariant(buffer);
        return new string(buffer);
    }
}

// Program.cs
class Program
{
    static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<StringBenchmarks>();
    }
}
```

## Rendimiento de Colecciones

```csharp
// Usar tipos de colección apropiados
public class CollectionExamples
{
    // Búsquedas rápidas: Dictionary sobre List
    private readonly Dictionary<int, Product> _productsById = new();

    // HashSet para elementos únicos
    private readonly HashSet<string> _processedIds = new();

    // Frozen collections para datos readonly (.NET 8)
    private static readonly FrozenDictionary<string, int> StatusCodes =
        new Dictionary<string, int>
        {
            ["Active"] = 1,
            ["Inactive"] = 0
        }.ToFrozenDictionary();

    // Pre-dimensionar colecciones cuando se conoce el count
    public List<Product> CreateProducts(int count)
    {
        var products = new List<Product>(count); // Pre-asignar
        for (int i = 0; i < count; i++)
        {
            products.Add(new Product { Id = i });
        }
        return products;
    }

    // Usar spans para operaciones de array
    public int SumArray(int[] numbers)
    {
        return Sum(numbers.AsSpan());
    }

    private static int Sum(ReadOnlySpan<int> numbers)
    {
        int total = 0;
        foreach (var n in numbers)
            total += n;
        return total;
    }
}
```

## Optimización de LINQ

```csharp
public class LinqOptimizations
{
    // Evitar múltiples enumeraciones
    public void BadExample(IEnumerable<int> numbers)
    {
        if (numbers.Any())
        {
            var first = numbers.First(); // Enumera de nuevo
            var count = numbers.Count(); // Enumera de nuevo
        }
    }

    public void GoodExample(IEnumerable<int> numbers)
    {
        var list = numbers.ToList(); // Enumerar una vez
        if (list.Count > 0)
        {
            var first = list[0];
            var count = list.Count;
        }
    }

    // Usar métodos LINQ apropiados
    public bool HasActiveUsers(List<User> users)
    {
        return users.Any(u => u.IsActive); // Mejor que Count() > 0
    }

    // Evitar ToList() innecesario
    public IEnumerable<Product> GetExpensiveProducts(IEnumerable<Product> products)
    {
        return products.Where(p => p.Price > 100); // Ejecución diferida
    }

    // Usar Select para proyecciones temprano
    public List<string> GetProductNames(IEnumerable<Product> products)
    {
        return products
            .Where(p => p.IsActive)
            .Select(p => p.Name) // Proyectar temprano
            .ToList();
    }
}
```

## Response Caching y Compresión

```csharp
// Program.cs
builder.Services.AddResponseCaching();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

app.UseResponseCompression();
app.UseResponseCaching();

// Endpoint con caching
app.MapGet("/api/products", async (ProductService service) =>
{
    var products = await service.GetAllAsync();
    return Results.Ok(products);
})
.CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(5)));
```

## Optimización de Consultas de Base de Datos

```csharp
public class OptimizedQueries(AppDbContext context)
{
    // Usar AsNoTracking para consultas de solo lectura
    public async Task<List<ProductDto>> GetProductsAsync(CancellationToken ct)
    {
        return await context.Products
            .AsNoTracking()
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price
            })
            .ToListAsync(ct);
    }

    // Evitar N+1 queries con Include
    public async Task<List<Order>> GetOrdersWithItemsAsync(CancellationToken ct)
    {
        return await context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // Usar consultas compiladas para queries repetidas
    private static readonly Func<AppDbContext, int, Task<Product?>> GetProductById =
        EF.CompileAsyncQuery((AppDbContext ctx, int id) =>
            ctx.Products.FirstOrDefault(p => p.Id == id));

    public Task<Product?> GetProductOptimizedAsync(int id)
    {
        return GetProductById(context, id);
    }

    // Paginación
    public async Task<PagedResult<ProductDto>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var query = context.Products.AsNoTracking();

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price
            })
            .ToListAsync(ct);

        return new PagedResult<ProductDto>(items, total, page, pageSize);
    }
}
```

## Source Generators y AOT

```csharp
// Preparar para Native AOT
using System.Text.Json.Serialization;

[JsonSerializable(typeof(ProductDto))]
[JsonSerializable(typeof(List<ProductDto>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}

// Uso en API
app.MapGet("/api/products", async (ProductService service) =>
{
    var products = await service.GetAllAsync();
    return Results.Json(products, AppJsonSerializerContext.Default.ListProductDto);
});

// .csproj para AOT
<PropertyGroup>
    <PublishAot>true</PublishAot>
    <InvariantGlobalization>true</InvariantGlobalization>
    <JsonSerializerIsReflectionEnabledByDefault>false</JsonSerializerIsReflectionEnabledByDefault>
</PropertyGroup>
```

## Consejos de Perfilado de Memoria

```csharp
// Evitar boxing de value types
public void AvoidBoxing()
{
    // Malo: boxing
    object obj = 42;

    // Bueno: usar generics
    void Print<T>(T value) => Console.WriteLine(value);
    Print(42); // Sin boxing
}

// Usar structs para datos pequeños e inmutables
public readonly struct Point(int x, int y)
{
    public int X { get; } = x;
    public int Y { get; } = y;
}

// Evitar concatenación de strings en loops
public string BuildString(List<string> items)
{
    var builder = new StringBuilder();
    foreach (var item in items)
    {
        builder.Append(item);
    }
    return builder.ToString();
}
```

## Referencia Rápida

| Optimización | Caso de Uso | Beneficio |
|-------------|----------|---------|
| `Span<T>` | Operaciones de array/string | Sin asignación |
| `ArrayPool<T>` | Buffers temporales | Reducir presión GC |
| `ValueTask<T>` | Caminos frecuentemente síncronos | Menor asignación |
| `ConfigureAwait(false)` | Librerías | Evitar captura de contexto |
| Frozen collections | Datos estáticos readonly | Búsquedas más rápidas |
| `AsNoTracking()` | Consultas solo lectura | Mejor rendimiento EF |
| Object pooling | Objetos pesados | Reutilizar instancias |
| Response caching | Respuestas estáticas | Reducir carga del servidor |
| Native AOT | Tiempo de inicio crítico | Inicio frío más rápido |
