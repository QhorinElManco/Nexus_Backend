# ASP.NET Core

Minimal APIs, middleware, autenticación y configuración.

## Minimal API - Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapProductEndpoints();
app.Run();
```

## Minimal API Endpoints

```csharp
public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products")
            .RequireAuthorization();

        group.MapGet("/", GetAllProducts)
            .WithName("GetProducts")
            .Produces<List<ProductDto>>();

        group.MapGet("/{id:int}", GetProductById)
            .Produces<ProductDto>()
            .Produces(404);

        group.MapPost("/", CreateProduct)
            .Produces<ProductDto>(201)
            .ProducesValidationProblem();

        group.MapPut("/{id:int}", UpdateProduct).Produces(204);
        group.MapDelete("/{id:int}", DeleteProduct).Produces(204);
    }

    private static async Task<IResult> GetAllProducts(ProductService service, CancellationToken ct)
        => Results.Ok(await service.GetAllAsync(ct));

    private static async Task<IResult> GetProductById(int id, ProductService service, CancellationToken ct)
    {
        var product = await service.GetByIdAsync(id, ct);
        return product is not null ? Results.Ok(product) : Results.NotFound();
    }
}
```

## Endpoint Filters

```csharp
public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<T>().FirstOrDefault();
        if (request is null) return Results.BadRequest();

        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is not null)
        {
            var result = await validator.ValidateAsync(request);
            if (!result.IsValid) return Results.ValidationProblem(result.ToDictionary());
        }
        return await next(context);
    }
}

// Uso
group.MapPost("/", CreateProduct)
    .AddEndpointFilter<ValidationFilter<CreateProductRequest>>();
```

## Middleware Personalizado

```csharp
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var start = DateTime.UtcNow;
        try { await next(context); }
        finally
        {
            logger.LogInformation("Request {Method} {Path} completed in {Elapsed}ms",
                context.Request.Method, context.Request.Path,
                (DateTime.UtcNow - start).TotalMilliseconds);
        }
    }
}

app.UseMiddleware<RequestLoggingMiddleware>();
// o
app.UseRequestLogging();
```

## Autenticación JWT

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidateAudience = true,
            ValidateLifetime = true, ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer, ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});
```

## Rate Limiting

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true, PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

app.UseRateLimiter();
```

## Output Caching

```csharp
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(b => b.Expire(TimeSpan.FromSeconds(10)));
    options.AddPolicy("Products", b => b.Expire(TimeSpan.FromMinutes(5)).SetVaryByQuery("page"));
});

app.UseOutputCache();
app.MapGet("/api/products", GetProducts).CacheOutput("Products");
```

## Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddUrlGroup(new Uri("https://api.example.com/health"), "External API");

app.MapHealthChecks("/health");
```

## Manejo de Excepciones

```csharp
app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled exception");

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "An error occurred" });
    });
});
```

## Referencia Rápida

| Patrón | Uso |
|--------|-----|
| Minimal API | Endpoints simples |
| Route Groups | Organizar endpoints |
| Endpoint Filters | Validación |
| Scoped Service | Por request HTTP |
| Singleton Service | Estado compartido |
| Output Caching | Rendimiento |
| Rate Limiting | Protección API |
