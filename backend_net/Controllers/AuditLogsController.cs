using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AuditLogsController : BaseController
{
    private readonly IAuditService _auditService;

    public AuditLogsController(IAuditService auditService)
    {
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    /// <summary>
    /// Get audit logs with optional filters
    /// Only Admin and Owner can view audit logs
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? entityType,
        [FromQuery] int? entityId,
        [FromQuery] int? userId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        try
        {
            // Only Admin and Owner can view audit logs
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view audit logs", 403);
            }

            var logs = await _auditService.GetAuditLogsAsync(entityType, entityId, userId, startDate, endDate);
            return HandleSuccess("Audit logs retrieved successfully", logs);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get audit log by ID
    /// Only Admin and Owner can view audit logs
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuditLogById(int id)
    {
        try
        {
            // Only Admin and Owner can view audit logs
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view audit logs", 403);
            }

            var log = await _auditService.GetAuditLogByIdAsync(id);
            if (log == null)
            {
                return HandleError("Audit log not found", 404);
            }
            return HandleSuccess("Audit log retrieved successfully", log);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }
}

