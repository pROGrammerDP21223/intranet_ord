using backend_net.Domain.Entities;

namespace backend_net.Services.Interfaces;

public interface IArchiveService
{
    System.Threading.Tasks.Task ArchiveOldClientsAsync(int daysOld);
    System.Threading.Tasks.Task ArchiveOldEnquiriesAsync(int daysOld);
    System.Threading.Tasks.Task ArchiveOldTicketsAsync(int daysOld);
    System.Threading.Tasks.Task ArchiveOldTransactionsAsync(int daysOld);
    System.Threading.Tasks.Task ArchiveOldAuditLogsAsync(int daysOld);
    System.Threading.Tasks.Task<IEnumerable<object>> GetArchivedDataAsync(string entityType, DateTime? fromDate = null, DateTime? toDate = null);
    System.Threading.Tasks.Task<bool> RestoreArchivedDataAsync(string entityType, int entityId);
}

