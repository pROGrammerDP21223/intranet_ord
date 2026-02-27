using backend_net.Domain.Entities;

namespace backend_net.Services.Interfaces;

public interface IAuditService
{
    System.Threading.Tasks.Task LogCreateAsync<T>(T entity, int? userId = null, string? ipAddress = null, string? userAgent = null) where T : BaseEntity;
    System.Threading.Tasks.Task LogUpdateAsync<T>(T entity, T originalEntity, int? userId = null, string? ipAddress = null, string? userAgent = null) where T : BaseEntity;
    System.Threading.Tasks.Task LogDeleteAsync<T>(T entity, int? userId = null, string? ipAddress = null, string? userAgent = null) where T : BaseEntity;
    System.Threading.Tasks.Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string? entityType = null, int? entityId = null, int? userId = null, DateTime? startDate = null, DateTime? endDate = null);
    System.Threading.Tasks.Task<AuditLog?> GetAuditLogByIdAsync(int id);
}

