using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BackupController : BaseController
{
    private readonly IBackupService _backupService;

    public BackupController(IBackupService backupService)
    {
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
    }

    /// <summary>
    /// Create a database backup
    /// Only Admin and Owner can create backups
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> CreateBackup([FromQuery] string? backupPath = null)
    {
        try
        {
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can create backups", 403);
            }

            var backupFilePath = await _backupService.CreateBackupAsync(backupPath);
            return HandleSuccess("Backup created successfully", new { backupFilePath });
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Restore a database backup
    /// Only Admin and Owner can restore backups
    /// </summary>
    [HttpPost("restore")]
    public async Task<IActionResult> RestoreBackup([FromBody] RestoreBackupRequest request)
    {
        try
        {
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can restore backups", 403);
            }

            var result = await _backupService.RestoreBackupAsync(request.BackupFilePath);
            if (result)
            {
                return HandleSuccess("Backup restored successfully", null);
            }
            else
            {
                return HandleError("Failed to restore backup", 500);
            }
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// List all backups (alternative endpoint)
    /// Only Admin and Owner can list backups
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> ListBackups([FromQuery] string? backupPath = null)
    {
        try
        {
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can list backups", 403);
            }

            var backups = await _backupService.ListBackupsAsync(backupPath);
            var backupsList = backups?.ToList() ?? new List<string>();
            return HandleSuccess("Backups retrieved successfully", backupsList);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Delete a backup
    /// Only Admin and Owner can delete backups
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> DeleteBackup([FromQuery] string backupFilePath)
    {
        try
        {
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can delete backups", 403);
            }

            var result = await _backupService.DeleteBackupAsync(backupFilePath);
            if (result)
            {
                return HandleSuccess("Backup deleted successfully", null);
            }
            else
            {
                return HandleError("Failed to delete backup", 500);
            }
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }
}

public class RestoreBackupRequest
{
    public string BackupFilePath { get; set; } = string.Empty;
}

