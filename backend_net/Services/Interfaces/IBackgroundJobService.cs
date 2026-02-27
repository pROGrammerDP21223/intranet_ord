namespace backend_net.Services.Interfaces;

public interface IBackgroundJobService
{
    string ScheduleTaskReminder(int taskId, DateTime reminderDate);
    bool DeleteJob(string jobId);
    string ScheduleDataArchiving(DateTime archiveDate);
    string ScheduleDatabaseBackup(DateTime backupTime);
}

