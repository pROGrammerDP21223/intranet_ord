using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ArchiveController : BaseController
{
    private readonly IArchiveService _archiveService;

    public ArchiveController(IArchiveService archiveService)
    {
        _archiveService = archiveService ?? throw new ArgumentNullException(nameof(archiveService));
    }

    /// <summary>
    /// Archive old clients
    /// Only Admin and Owner can archive data
    /// </summary>
    [HttpPost("clients")]
    public async Task<IActionResult> ArchiveOldClients([FromBody] ArchiveRequest request)
    {
        try
        {
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can archive data", 403);
            }

            await _archiveService.ArchiveOldClientsAsync(request.DaysOld);
            return HandleSuccess("Clients archived successfully", null);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Archive old enquiries
    /// </summary>
    [HttpPost("enquiries")]
    public async Task<IActionResult> ArchiveOldEnquiries([FromBody] ArchiveRequest request)
    {
        try
        {
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can archive data", 403);
            }

            await _archiveService.ArchiveOldEnquiriesAsync(request.DaysOld);
            return HandleSuccess("Enquiries archived successfully", null);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Archive old tickets
    /// </summary>
    [HttpPost("tickets")]
    public async Task<IActionResult> ArchiveOldTickets([FromBody] ArchiveRequest request)
    {
        try
        {
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can archive data", 403);
            }

            await _archiveService.ArchiveOldTicketsAsync(request.DaysOld);
            return HandleSuccess("Tickets archived successfully", null);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Archive old transactions
    /// </summary>
    [HttpPost("transactions")]
    public async Task<IActionResult> ArchiveOldTransactions([FromBody] ArchiveRequest request)
    {
        try
        {
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can archive data", 403);
            }

            await _archiveService.ArchiveOldTransactionsAsync(request.DaysOld);
            return HandleSuccess("Transactions archived successfully", null);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Archive old audit logs
    /// </summary>
    [HttpPost("audit-logs")]
    public async Task<IActionResult> ArchiveOldAuditLogs([FromBody] ArchiveRequest request)
    {
        try
        {
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can archive data", 403);
            }

            await _archiveService.ArchiveOldAuditLogsAsync(request.DaysOld);
            return HandleSuccess("Audit logs archived successfully", null);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// Get archived data for a specific entity type
    /// </summary>
    [HttpGet("{entityType}")]
    public async Task<IActionResult> GetArchivedData(
        string entityType,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view archived data", 403);
            }

            var archivedData = await _archiveService.GetArchivedDataAsync(entityType, fromDate, toDate);
            var archivedDataList = archivedData?.ToList() ?? new List<object>();
            return HandleSuccess("Archived data retrieved successfully", archivedDataList);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Restore archived data
    /// </summary>
    [HttpPost("restore/{entityType}/{entityId}")]
    public async Task<IActionResult> RestoreArchivedData(string entityType, int entityId)
    {
        try
        {
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can restore archived data", 403);
            }

            var result = await _archiveService.RestoreArchivedDataAsync(entityType, entityId);
            if (result)
            {
                return HandleSuccess("Archived data restored successfully", null);
            }
            else
            {
                return HandleError("Failed to restore archived data", 500);
            }
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }
}

public class ArchiveRequest
{
    public int DaysOld { get; set; } = 365;
}

