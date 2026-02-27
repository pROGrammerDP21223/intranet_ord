using backend_net.Data.Context;
using backend_net.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace backend_net.Services;

public class BackupService : IBackupService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BackupService> _logger;
    private readonly string _backupDirectory;

    public BackupService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<BackupService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Default backup directory
        _backupDirectory = Path.Combine(
            Directory.GetCurrentDirectory(),
            "backups"
        );

        // Ensure backup directory exists
        if (!Directory.Exists(_backupDirectory))
        {
            Directory.CreateDirectory(_backupDirectory);
        }
    }

    public async System.Threading.Tasks.Task<string> CreateBackupAsync(string? backupPath = null)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Database connection string not found");
            }

            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;

            if (string.IsNullOrEmpty(backupPath))
            {
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                backupPath = Path.Combine(_backupDirectory, $"backup_{databaseName}_{timestamp}.bak");
            }

            var sql = $@"
                BACKUP DATABASE [{databaseName}]
                TO DISK = '{backupPath}'
                WITH FORMAT, INIT, NAME = 'Full Backup of {databaseName}',
                SKIP, NOREWIND, NOUNLOAD, STATS = 10";

            await _context.Database.ExecuteSqlRawAsync(sql);

            _logger.LogInformation("Database backup created successfully at {BackupPath}", backupPath);
            return backupPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create database backup");
            throw;
        }
    }

    public async System.Threading.Tasks.Task<bool> RestoreBackupAsync(string backupFilePath)
    {
        try
        {
            if (!File.Exists(backupFilePath))
            {
                throw new FileNotFoundException($"Backup file not found: {backupFilePath}");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Database connection string not found");
            }

            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;

            // Set database to single user mode
            var setSingleUserSql = $@"
                ALTER DATABASE [{databaseName}]
                SET SINGLE_USER WITH ROLLBACK IMMEDIATE";

            await _context.Database.ExecuteSqlRawAsync(setSingleUserSql);

            try
            {
                // Restore database
                var restoreSql = $@"
                    RESTORE DATABASE [{databaseName}]
                    FROM DISK = '{backupFilePath}'
                    WITH REPLACE, RECOVERY";

                await _context.Database.ExecuteSqlRawAsync(restoreSql);

                _logger.LogInformation("Database restored successfully from {BackupFilePath}", backupFilePath);
                return true;
            }
            finally
            {
                // Set database back to multi-user mode
                var setMultiUserSql = $@"
                    ALTER DATABASE [{databaseName}]
                    SET MULTI_USER";

                await _context.Database.ExecuteSqlRawAsync(setMultiUserSql);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore database backup from {BackupFilePath}", backupFilePath);
            throw;
        }
    }

    public async System.Threading.Tasks.Task<IEnumerable<string>> ListBackupsAsync(string? backupPath = null)
    {
        try
        {
            var directory = backupPath ?? _backupDirectory;
            if (!Directory.Exists(directory))
            {
                return Enumerable.Empty<string>();
            }

            var backups = Directory.GetFiles(directory, "*.bak")
                .OrderByDescending(f => new FileInfo(f).CreationTime)
                .ToList();

            return await System.Threading.Tasks.Task.FromResult(backups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list backups from {BackupPath}", backupPath);
            return Enumerable.Empty<string>();
        }
    }

    public async System.Threading.Tasks.Task<bool> DeleteBackupAsync(string backupFilePath)
    {
        try
        {
            if (!File.Exists(backupFilePath))
            {
                return false;
            }

            File.Delete(backupFilePath);
            _logger.LogInformation("Backup file deleted: {BackupFilePath}", backupFilePath);
            return await System.Threading.Tasks.Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete backup file {BackupFilePath}", backupFilePath);
            return false;
        }
    }
}

