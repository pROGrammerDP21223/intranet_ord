namespace backend_net.Services.Interfaces;

public interface IBackupService
{
    System.Threading.Tasks.Task<string> CreateBackupAsync(string? backupPath = null);
    System.Threading.Tasks.Task<bool> RestoreBackupAsync(string backupFilePath);
    System.Threading.Tasks.Task<IEnumerable<string>> ListBackupsAsync(string? backupPath = null);
    System.Threading.Tasks.Task<bool> DeleteBackupAsync(string backupFilePath);
}

