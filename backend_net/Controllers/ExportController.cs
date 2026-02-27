using backend_net.Data.Context;
using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ExportController : BaseController
{
    private readonly IExportService _exportService;
    private readonly ApplicationDbContext _context;
    private readonly IAccessControlService _accessControlService;

    public ExportController(
        IExportService exportService,
        ApplicationDbContext context,
        IAccessControlService accessControlService)
    {
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _accessControlService = accessControlService ?? throw new ArgumentNullException(nameof(accessControlService));
    }

    [HttpGet("clients/excel")]
    public async Task<IActionResult> ExportClientsToExcel()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var accessibleClientIds = await _accessControlService.GetAccessibleClientIdsAsync(userId.Value);
            var clients = await _context.Clients
                .Where(c => !c.IsDeleted && (!accessibleClientIds.Any() || accessibleClientIds.Contains(c.Id)))
                .ToListAsync();

            var excelData = await _exportService.ExportClientsToExcelAsync(clients);
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"clients_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("clients/csv")]
    public async Task<IActionResult> ExportClientsToCsv()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var accessibleClientIds = await _accessControlService.GetAccessibleClientIdsAsync(userId.Value);
            var clients = await _context.Clients
                .Where(c => !c.IsDeleted && (!accessibleClientIds.Any() || accessibleClientIds.Contains(c.Id)))
                .ToListAsync();

            var csvData = await _exportService.ExportClientsToCsvAsync(clients);
            return File(csvData, "text/csv", $"clients_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("enquiries/excel")]
    public async Task<IActionResult> ExportEnquiriesToExcel([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var accessibleClientIds = await _accessControlService.GetAccessibleClientIdsAsync(userId.Value);

            var query = _context.Enquiries
                .Include(e => e.Client)
                .Where(e => !e.IsDeleted);

            if (accessibleClientIds.Any())
            {
                query = query.Where(e => accessibleClientIds.Contains(e.ClientId));
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.CreatedAt <= endDate.Value);
            }

            var enquiries = await query.ToListAsync();

            var excelData = await _exportService.ExportEnquiriesToExcelAsync(enquiries);
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"enquiries_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("enquiries/csv")]
    public async Task<IActionResult> ExportEnquiriesToCsv([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var accessibleClientIds = await _accessControlService.GetAccessibleClientIdsAsync(userId.Value);

            var query = _context.Enquiries
                .Include(e => e.Client)
                .Where(e => !e.IsDeleted);

            if (accessibleClientIds.Any())
            {
                query = query.Where(e => accessibleClientIds.Contains(e.ClientId));
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.CreatedAt <= endDate.Value);
            }

            var enquiries = await query.ToListAsync();

            var csvData = await _exportService.ExportEnquiriesToCsvAsync(enquiries);
            return File(csvData, "text/csv", $"enquiries_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("transactions/excel")]
    public async Task<IActionResult> ExportTransactionsToExcel([FromQuery] int? clientId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var accessibleClientIds = await _accessControlService.GetAccessibleClientIdsAsync(userId.Value);

            var query = _context.Transactions
                .Include(t => t.Client)
                .Where(t => !t.IsDeleted);

            if (clientId.HasValue)
            {
                if (!accessibleClientIds.Any() || !accessibleClientIds.Contains(clientId.Value))
                {
                    return HandleError("Unauthorized: You don't have access to this client's transactions", 403);
                }
                query = query.Where(t => t.ClientId == clientId.Value);
            }
            else if (accessibleClientIds.Any())
            {
                query = query.Where(t => accessibleClientIds.Contains(t.ClientId));
            }

            if (startDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate <= endDate.Value);
            }

            var transactions = await query.ToListAsync();

            var excelData = await _exportService.ExportTransactionsToExcelAsync(transactions);
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"transactions_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("transactions/csv")]
    public async Task<IActionResult> ExportTransactionsToCsv([FromQuery] int? clientId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var accessibleClientIds = await _accessControlService.GetAccessibleClientIdsAsync(userId.Value);

            var query = _context.Transactions
                .Include(t => t.Client)
                .Where(t => !t.IsDeleted);

            if (clientId.HasValue)
            {
                if (!accessibleClientIds.Any() || !accessibleClientIds.Contains(clientId.Value))
                {
                    return HandleError("Unauthorized: You don't have access to this client's transactions", 403);
                }
                query = query.Where(t => t.ClientId == clientId.Value);
            }
            else if (accessibleClientIds.Any())
            {
                query = query.Where(t => accessibleClientIds.Contains(t.ClientId));
            }

            if (startDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate <= endDate.Value);
            }

            var transactions = await query.ToListAsync();

            var csvData = await _exportService.ExportTransactionsToCsvAsync(transactions);
            return File(csvData, "text/csv", $"transactions_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }
}
