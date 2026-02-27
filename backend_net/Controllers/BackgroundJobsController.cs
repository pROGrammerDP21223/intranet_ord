using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BackgroundJobsController : BaseController
{
    private readonly IBackgroundJobService _backgroundJobService;

    public BackgroundJobsController(IBackgroundJobService backgroundJobService)
    {
        _backgroundJobService = backgroundJobService ?? throw new ArgumentNullException(nameof(backgroundJobService));
    }

    /// <summary>
    /// Schedule a task reminder
    /// </summary>
    [HttpPost("schedule-task-reminder")]
    public async Task<IActionResult> ScheduleTaskReminder([FromBody] ScheduleTaskReminderRequest request)
    {
        try
        {
            var jobId = _backgroundJobService.ScheduleTaskReminder(request.TaskId, request.ReminderDate);
            return HandleSuccess("Task reminder scheduled successfully", new { jobId });
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Schedule a database backup
    /// Only Admin and Owner can schedule backups
    /// </summary>
    [HttpPost("schedule-backup")]
    public async Task<IActionResult> ScheduleBackup([FromBody] ScheduleBackupRequest request)
    {
        try
        {
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can schedule backups", 403);
            }

            var jobId = _backgroundJobService.ScheduleDatabaseBackup(request.BackupTime);
            return HandleSuccess("Database backup scheduled successfully", new { jobId });
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Schedule data archiving
    /// Only Admin and Owner can schedule archiving
    /// </summary>
    [HttpPost("schedule-archiving")]
    public async Task<IActionResult> ScheduleArchiving([FromBody] ScheduleArchivingRequest request)
    {
        try
        {
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can schedule archiving", 403);
            }

            var jobId = _backgroundJobService.ScheduleDataArchiving(request.ArchiveDate);
            return HandleSuccess("Data archiving scheduled successfully", new { jobId });
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Delete a scheduled job
    /// </summary>
    [HttpDelete("{jobId}")]
    public async Task<IActionResult> DeleteJob(string jobId)
    {
        try
        {
            var result = _backgroundJobService.DeleteJob(jobId);
            if (result)
            {
                return HandleSuccess("Job deleted successfully", null);
            }
            else
            {
                return HandleError("Failed to delete job", 500);
            }
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }
}

public class ScheduleBackupRequest
{
    public DateTime BackupTime { get; set; }
}

public class ScheduleTaskReminderRequest
{
    public int TaskId { get; set; }
    public DateTime ReminderDate { get; set; }
}

public class ScheduleArchivingRequest
{
    public DateTime ArchiveDate { get; set; }
}

