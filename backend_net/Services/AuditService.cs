using backend_net.Data.Context;
using backend_net.Domain.Entities;
using backend_net.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace backend_net.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task LogCreateAsync<T>(T entity, int? userId = null, string? ipAddress = null, string? userAgent = null) where T : BaseEntity
    {
        try
        {
            var auditLog = await CreateAuditLogAsync(entity, "Create", null, null, userId, ipAddress, userAgent);
            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log create action for {EntityType} {EntityId}", typeof(T).Name, entity.Id);
        }
    }

    public async System.Threading.Tasks.Task LogUpdateAsync<T>(T entity, T originalEntity, int? userId = null, string? ipAddress = null, string? userAgent = null) where T : BaseEntity
    {
        try
        {
            var oldValues = SerializeEntity(originalEntity);
            var newValues = SerializeEntity(entity);
            var changes = GetChangesSummary(originalEntity, entity);

            var auditLog = await CreateAuditLogAsync(entity, "Update", oldValues, newValues, userId, ipAddress, userAgent);
            auditLog.Changes = changes;
            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log update action for {EntityType} {EntityId}", typeof(T).Name, entity.Id);
        }
    }

    public async System.Threading.Tasks.Task LogDeleteAsync<T>(T entity, int? userId = null, string? ipAddress = null, string? userAgent = null) where T : BaseEntity
    {
        try
        {
            var oldValues = SerializeEntity(entity);
            var auditLog = await CreateAuditLogAsync(entity, "Delete", oldValues, null, userId, ipAddress, userAgent);
            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log delete action for {EntityType} {EntityId}", typeof(T).Name, entity.Id);
        }
    }

    public async System.Threading.Tasks.Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string? entityType = null, int? entityId = null, int? userId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(entityType))
        {
            query = query.Where(a => a.EntityType == entityType);
        }

        if (entityId.HasValue)
        {
            query = query.Where(a => a.EntityId == entityId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(a => a.UserId == userId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt <= endDate.Value);
        }

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<AuditLog?> GetAuditLogByIdAsync(int id)
    {
        return await _context.AuditLogs
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    private async System.Threading.Tasks.Task<AuditLog> CreateAuditLogAsync<T>(T entity, string action, string? oldValues, string? newValues, int? userId, string? ipAddress, string? userAgent) where T : BaseEntity
    {
        var auditLog = new AuditLog
        {
            EntityType = typeof(T).Name,
            EntityId = entity.Id,
            Action = action,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        if (userId.HasValue)
        {
            auditLog.UserId = userId.Value;
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.Value && !u.IsDeleted);
            if (user != null)
            {
                auditLog.UserName = user.Name;
                auditLog.UserEmail = user.Email;
            }
        }

        return auditLog;
    }

    private string SerializeEntity<T>(T entity) where T : BaseEntity
    {
        try
        {
            // Serialize only public properties, excluding navigation properties
            var properties = typeof(T).GetProperties()
                .Where(p => p.CanRead && !p.PropertyType.IsClass || p.PropertyType == typeof(string))
                .Where(p => p.Name != "Id" && !p.Name.Contains("Navigation") && !p.Name.EndsWith("Id"))
                .ToDictionary(p => p.Name, p => p.GetValue(entity)?.ToString() ?? "");

            return JsonSerializer.Serialize(properties);
        }
        catch
        {
            return "{}";
        }
    }

    private string? GetChangesSummary<T>(T original, T updated) where T : BaseEntity
    {
        try
        {
            var changes = new List<string>();
            var properties = typeof(T).GetProperties()
                .Where(p => p.CanRead && (p.PropertyType.IsPrimitive || p.PropertyType == typeof(string) || p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?)));

            foreach (var prop in properties)
            {
                var oldValue = prop.GetValue(original)?.ToString();
                var newValue = prop.GetValue(updated)?.ToString();

                if (oldValue != newValue)
                {
                    changes.Add($"{prop.Name}: {oldValue} → {newValue}");
                }
            }

            return changes.Any() ? string.Join("; ", changes) : null;
        }
        catch
        {
            return null;
        }
    }
}

