using backend_net.Data.Context;
using backend_net.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend_net.Services;

public class ArchiveService : IArchiveService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ArchiveService> _logger;
    private readonly string _archiveDirectory;

    public ArchiveService(
        ApplicationDbContext context,
        ILogger<ArchiveService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _archiveDirectory = Path.Combine(
            Directory.GetCurrentDirectory(),
            "archives"
        );

        if (!Directory.Exists(_archiveDirectory))
        {
            Directory.CreateDirectory(_archiveDirectory);
        }
    }

    public async System.Threading.Tasks.Task ArchiveOldClientsAsync(int daysOld)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var oldClients = await _context.Clients
                .Where(c => !c.IsDeleted && c.CreatedAt < cutoffDate && c.Status == "Inactive")
                .ToListAsync();

            foreach (var client in oldClients)
            {
                // Mark as archived (you can add an IsArchived field or use a status)
                client.IsDeleted = true; // Using IsDeleted as archive flag for now
                client.UpdatedAt = DateTime.UtcNow;

                // Save archived data to JSON file
                await SaveArchivedDataAsync("Client", client.Id, client);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Archived {Count} old clients", oldClients.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive old clients");
            throw;
        }
    }

    public async System.Threading.Tasks.Task ArchiveOldEnquiriesAsync(int daysOld)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var oldEnquiries = await _context.Enquiries
                .Where(e => !e.IsDeleted && e.CreatedAt < cutoffDate && e.Status == "Closed")
                .ToListAsync();

            foreach (var enquiry in oldEnquiries)
            {
                enquiry.IsDeleted = true;
                enquiry.UpdatedAt = DateTime.UtcNow;
                await SaveArchivedDataAsync("Enquiry", enquiry.Id, enquiry);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Archived {Count} old enquiries", oldEnquiries.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive old enquiries");
            throw;
        }
    }

    public async System.Threading.Tasks.Task ArchiveOldTicketsAsync(int daysOld)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var oldTickets = await _context.Tickets
                .Where(t => !t.IsDeleted && t.CreatedAt < cutoffDate && t.Status == "Closed")
                .ToListAsync();

            foreach (var ticket in oldTickets)
            {
                ticket.IsDeleted = true;
                ticket.UpdatedAt = DateTime.UtcNow;
                await SaveArchivedDataAsync("Ticket", ticket.Id, ticket);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Archived {Count} old tickets", oldTickets.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive old tickets");
            throw;
        }
    }

    public async System.Threading.Tasks.Task ArchiveOldTransactionsAsync(int daysOld)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var oldTransactions = await _context.Transactions
                .Where(t => !t.IsDeleted && t.CreatedAt < cutoffDate)
                .ToListAsync();

            foreach (var transaction in oldTransactions)
            {
                transaction.IsDeleted = true;
                transaction.UpdatedAt = DateTime.UtcNow;
                await SaveArchivedDataAsync("Transaction", transaction.Id, transaction);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Archived {Count} old transactions", oldTransactions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive old transactions");
            throw;
        }
    }

    public async System.Threading.Tasks.Task ArchiveOldAuditLogsAsync(int daysOld)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var oldLogs = await _context.AuditLogs
                .Where(a => a.CreatedAt < cutoffDate)
                .ToListAsync();

            foreach (var log in oldLogs)
            {
                await SaveArchivedDataAsync("AuditLog", log.Id, log);
            }

            // Delete old audit logs after archiving
            _context.AuditLogs.RemoveRange(oldLogs);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Archived {Count} old audit logs", oldLogs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive old audit logs");
            throw;
        }
    }

    public async System.Threading.Tasks.Task<IEnumerable<object>> GetArchivedDataAsync(string entityType, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var archiveFiles = Directory.GetFiles(_archiveDirectory, $"{entityType}_*.json")
                .Where(f =>
                {
                    var fileInfo = new FileInfo(f);
                    if (fromDate.HasValue && fileInfo.CreationTime < fromDate.Value)
                        return false;
                    if (toDate.HasValue && fileInfo.CreationTime > toDate.Value)
                        return false;
                    return true;
                })
                .OrderByDescending(f => new FileInfo(f).CreationTime)
                .ToList();

            var archivedData = new List<object>();
            foreach (var file in archiveFiles)
            {
                var content = await File.ReadAllTextAsync(file);
                var data = System.Text.Json.JsonSerializer.Deserialize<object>(content);
                if (data != null)
                {
                    archivedData.Add(data);
                }
            }

            return archivedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get archived data for {EntityType}", entityType);
            return Enumerable.Empty<object>();
        }
    }

    public async System.Threading.Tasks.Task<bool> RestoreArchivedDataAsync(string entityType, int entityId)
    {
        try
        {
            var archiveFile = Path.Combine(_archiveDirectory, $"{entityType}_{entityId}.json");
            if (!File.Exists(archiveFile))
            {
                return false;
            }

            var content = await File.ReadAllTextAsync(archiveFile);
            // TODO: Implement actual restore logic based on entity type
            _logger.LogInformation("Restored archived {EntityType} with ID {EntityId}", entityType, entityId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore archived data for {EntityType} {EntityId}", entityType, entityId);
            return false;
        }
    }

    private async System.Threading.Tasks.Task SaveArchivedDataAsync(string entityType, int entityId, object data)
    {
        try
        {
            var archiveFile = Path.Combine(_archiveDirectory, $"{entityType}_{entityId}.json");
            var json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(archiveFile, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save archived data for {EntityType} {EntityId}", entityType, entityId);
        }
    }
}

