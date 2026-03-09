using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Nexos.Domain.Entity;
using Nexos.Domain.Entity.Audit;

namespace Nexos.Persistence.Interceptors;

public class SaveChangesInterceptor : ISaveChangesInterceptor
{
    private static readonly HashSet<string> EntitiesToAudit =
    [
        nameof(Domain.Entity.Customers.Customer),
        nameof(Domain.Entity.Products.Product),
        nameof(Domain.Entity.Sales.Order),
        nameof(Domain.Entity.Transactions.KardexEntry)
    ];

    private static readonly HashSet<string> ExcludedTables =
    [
        nameof(Domain.Entity.Security.Company),
        nameof(Domain.Entity.Security.Role),
        nameof(Domain.Entity.Security.Access),
        nameof(Domain.Entity.Security.UserRole),
        nameof(Domain.Entity.Security.RoleAccess),
        nameof(AuditLog)
    ];

    private readonly Dictionary<EntityEntry, Dictionary<string, object?>> _originalValues = new();

    public ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return ValueTask.FromResult(result);
        }

        CaptureOriginalValues(eventData.Context);
        return ValueTask.FromResult(result);
    }

    public InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is null)
        {
            return result;
        }

        CaptureOriginalValues(eventData.Context);
        return result;
    }

    public async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return result;
        }

        await SaveAuditLogsAsync(eventData.Context, cancellationToken);
        return result;
    }

    public int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        if (eventData.Context is null)
        {
            return result;
        }

        SaveAuditLogs(eventData.Context);
        return result;
    }

    private void CaptureOriginalValues(DbContext context)
    {
        _originalValues.Clear();

        var entries = context.ChangeTracker.Entries()
            .Where(e => EntitiesToAudit.Contains(e.Metadata.ClrType.Name) &&
                        (e.State == EntityState.Modified || e.State == EntityState.Deleted));

        foreach (var entry in entries)
        {
            var originalValues = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.IsPrimaryKey() ||
                    property.Metadata.Name == nameof(BaseEntity.CreatedAt) ||
                    property.Metadata.Name == nameof(BaseEntity.IsDeleted))
                {
                    continue;
                }

                originalValues[property.Metadata.Name] = property.OriginalValue;
            }

            _originalValues[entry] = originalValues;
        }
    }

    private async Task SaveAuditLogsAsync(DbContext context, CancellationToken cancellationToken)
    {
        var auditLogs = CreateAuditLogs(context);

        foreach (var auditLog in auditLogs)
        {
            context.Set<AuditLog>().Add(auditLog);
        }

        if (auditLogs.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private void SaveAuditLogs(DbContext context)
    {
        var auditLogs = CreateAuditLogs(context);

        foreach (var auditLog in auditLogs)
        {
            context.Set<AuditLog>().Add(auditLog);
        }

        if (auditLogs.Count > 0)
        {
            context.SaveChanges();
        }
    }

    private List<AuditLog> CreateAuditLogs(DbContext context)
    {
        var auditLogs = new List<AuditLog>();
        var entries = context.ChangeTracker.Entries()
            .Where(e => EntitiesToAudit.Contains(e.Metadata.ClrType.Name) &&
                        (e.State == EntityState.Added || e.State == EntityState.Modified ||
                         e.State == EntityState.Deleted));

        foreach (var entry in entries)
        {
            var entityName = entry.Metadata.ClrType.Name;
            var action = GetAction(entry.State);

            if (action is null)
            {
                continue;
            }

            string? oldData = null;
            string? newData = null;

            if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
            {
                if (_originalValues.TryGetValue(entry, out var original))
                {
                    oldData = JsonSerializer.Serialize(original);
                }
            }

            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                var currentValues = new Dictionary<string, object?>();
                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.IsPrimaryKey() ||
                        property.Metadata.Name == nameof(BaseEntity.CreatedAt) ||
                        property.Metadata.Name == nameof(BaseEntity.IsDeleted))
                    {
                        continue;
                    }

                    currentValues[property.Metadata.Name] = property.CurrentValue;
                }

                newData = JsonSerializer.Serialize(currentValues);
            }

            var primaryKey = GetPrimaryKeyValue(entry);

            // TODO: Obtener CompanyId y UserId del contexto de autenticación cuando se implemente
            var auditLog = new AuditLog
            {
                CompanyId = 1, // TODO: Reemplazar con CompanyId del usuario autenticado
                UserId = null, // TODO: Asignar UserId del ClaimsPrincipal cuando exista autenticación
                ModuleName = entityName,
                Action = action,
                OldData = oldData,
                NewData = newData,
                RiskLevel = GetRiskLevel(entry.State),
                CreatedAt = DateTime.UtcNow
            };

            auditLogs.Add(auditLog);
        }

        return auditLogs;
    }

    private static string? GetAction(EntityState state)
    {
        return state switch
        {
            EntityState.Added => "INSERT",
            EntityState.Modified => "UPDATE",
            EntityState.Deleted => "DELETE",
            _ => null
        };
    }

    private static string GetRiskLevel(EntityState state)
    {
        return state switch
        {
            EntityState.Deleted => "High",
            EntityState.Modified => "Medium",
            EntityState.Added => "Low",
            _ => "Low"
        };
    }

    private static object? GetPrimaryKeyValue(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key is null)
        {
            return null;
        }

        var keyValues = new object[key.Properties.Count];
        for (var i = 0; i < key.Properties.Count; i++)
        {
            keyValues[i] = entry.Property(key.Properties[i].Name).CurrentValue!;
        }

        return keyValues.Length == 1 ? keyValues[0] : keyValues;
    }
}
