# Entity Framework Core

Patrones y mejores prácticas para EF Core.

## DbContext

```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
    }
}
```

## Configuración de Entidades

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Price).HasPrecision(18, 2);
        
        builder.HasIndex(p => p.Sku).IsUnique();
        
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

## Entidades Base

```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}

public class Product : BaseEntity
{
    public required string Name { get; set; }
    public required string Sku { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
```

## Optimización de Consultas

### AsNoTracking - Solo Lectura

```csharp
// ✅ Siempre usar para lecturas
var products = await _context.Products
    .AsNoTracking()
    .Where(p => p.CategoryId == categoryId)
    .ToListAsync(ct);
```

### Proyección a DTO

```csharp
// ✅ Traer solo columnas necesarias
var products = await _context.Products
    .AsNoTracking()
    .Select(p => new ProductDto
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price
    })
    .ToListAsync(ct);
```

### Eager Loading - Evitar N+1

```csharp
// ✅ Query único con Include
var orders = await _context.Orders
    .AsNoTracking()
    .Include(o => o.Items)
        .ThenInclude(i => i.Product)
    .Where(o => o.CustomerId == customerId)
    .ToListAsync(ct);
```

### Split Queries

```csharp
// ✅ Evita explosión cartesiana con múltiples Includes
var orders = await _context.Orders
    .AsNoTracking()
    .Include(o => o.Items)
    .Include(o => o.Payments)
    .AsSplitQuery()
    .ToListAsync(ct);
```

### Consultas Compiladas

```csharp
private static readonly Func<AppDbContext, int, Task<Product?>> GetByIdQuery =
    EF.CompileAsyncQuery((AppDbContext ctx, int id) =>
        ctx.Products.AsNoTracking().FirstOrDefault(p => p.Id == id));

public Task<Product?> GetByIdAsync(int id) => GetByIdQuery(_context, id);
```

## Operaciones Bulk

### ExecuteUpdate/ExecuteDelete (.NET 7+)

```csharp
// UPDATE sin cargar entidades
await _context.Products
    .Where(p => p.CategoryId == oldCat)
    .ExecuteUpdateAsync(s => s
        .SetProperty(p => p.CategoryId, newCat)
        .SetProperty(p => p.UpdatedAt, DateTime.UtcNow), ct);

// DELETE sin cargar entidades
await _context.Products
    .Where(p => p.IsDeleted)
    .ExecuteDeleteAsync(ct);
```

### Bulk Insert

```csharp
// Usar EFCore.BulkExtensions
await _context.BulkInsertAsync(products, ct);
```

## Transacciones

```csharp
using var transaction = await _context.Database.BeginTransactionAsync(ct);
try
{
    await _context.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);
}
catch
{
    await transaction.RollbackAsync(ct);
    throw;
}
```

## Concurrencia - Row Versioning

```csharp
public class Product
{
    [Timestamp]
    public byte[] RowVersion { get; set; }
}

// Manejar conflicto
catch (DbUpdateConcurrencyException ex)
{
    var entry = ex.Entries.Single();
    var dbValues = await entry.GetDatabaseValuesAsync(ct);
    entry.OriginalValues.SetValues(dbValues);
    await _context.SaveChangesAsync(ct);
}
```

## Configuración de Conexión

```csharp
// Connection pooling
services.AddDbContextPool<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
}, poolSize: 128);

// Con reintentos
services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
        sqlOptions.CommandTimeout(30);
    });
});
```

## Índices

```csharp
builder.HasIndex(p => p.Sku).IsUnique();
builder.HasIndex(p => new { p.CategoryId, p.Name });
builder.HasIndex(p => p.Price).HasFilter("[IsDeleted] = 0");
```

## Anti-Patrones a Evitar

```csharp
// ❌ ToList() antes de filtrar
var products = _context.Products.ToList().Where(...);

// ✅ Filtrar en SQL
var products = await _context.Products.Where(...).ToListAsync(ct);

// ❌ Contains con miles de IDs
var products = await _context.Products.Where(p => ids.Contains(p.Id)).ToListAsync(ct);

// ✅ Usar lotes
foreach (var batch in ids.Chunk(100))
{
    var results = await _context.Products.Where(p => batch.Contains(p.Id)).ToListAsync(ct);
}
```

## Migraciones

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet ef migrations script
dotnet ef database update PreviousMigration
```

## Referencia Rápida

| Operación | Método |
|-----------|--------|
| Solo lectura | `.AsNoTracking()` |
| Eager loading | `.Include()` |
| Split query | `.AsSplitQuery()` |
| Bulk update | `.ExecuteUpdateAsync()` |
| Bulk delete | `.ExecuteDeleteAsync()` |
| Query compilada | `EF.CompileAsyncQuery()` |
| Soft delete | `HasQueryFilter()` |
