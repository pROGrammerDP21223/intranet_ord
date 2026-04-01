using backend_net.Data.Context;
using backend_net.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Controllers;

[Route("api/free-registrations")]
[ApiController]
public class FreeRegistrationsController : BaseController
{
    private readonly ApplicationDbContext _context;

    public FreeRegistrationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /api/free-registrations  (Admin, Owner, SalesManager)
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null)
    {
        if (!IsAdmin() && !IsOwner() && !IsSalesManager())
            return HandleError("Unauthorized", 403);

        var query = _context.FreeRegistrations
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status == status);

        var list = await query.Select(r => new {
            r.Id, r.CompanyName, r.ContactPerson, r.Designation,
            r.Address, r.Phone, r.Email, r.WhatsAppNumber, r.DomainName,
            r.ProductsInterested, r.Status, r.ApprovedBy, r.ApprovedAt,
            r.RejectionReason, r.Notes, r.CreatedAt, r.UpdatedAt
        }).ToListAsync();

        return Ok(new { success = true, data = list });
    }

    // GET /api/free-registrations/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        if (!IsAdmin() && !IsOwner() && !IsSalesManager())
            return HandleError("Unauthorized", 403);

        var reg = await _context.FreeRegistrations
            .Where(r => r.Id == id && !r.IsDeleted)
            .Select(r => new {
                r.Id, r.CompanyName, r.ContactPerson, r.Designation,
                r.Address, r.Phone, r.Email, r.WhatsAppNumber, r.DomainName,
                r.ProductsInterested, r.Status, r.ApprovedBy, r.ApprovedAt,
                r.RejectionReason, r.Notes, r.CreatedAt, r.UpdatedAt
            }).FirstOrDefaultAsync();

        if (reg == null) return HandleError("Not found", 404);
        return Ok(new { success = true, data = reg });
    }

    // POST /api/free-registrations/{id}/approve
    [HttpPost("{id}/approve")]
    [Authorize]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveRequest? body)
    {
        if (!IsAdmin() && !IsOwner() && !IsSalesManager())
            return HandleError("Unauthorized", 403);

        var reg = await _context.FreeRegistrations.FindAsync(id);
        if (reg == null || reg.IsDeleted) return HandleError("Not found", 404);

        reg.Status = "Approved";
        reg.ApprovedBy = GetCurrentUserName() ?? "Admin";
        reg.ApprovedAt = DateTime.UtcNow;
        reg.Notes = body?.Notes ?? reg.Notes;
        reg.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { success = true, message = "Registration approved." });
    }

    // POST /api/free-registrations/{id}/reject
    [HttpPost("{id}/reject")]
    [Authorize]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectRequest? body)
    {
        if (!IsAdmin() && !IsOwner() && !IsSalesManager())
            return HandleError("Unauthorized", 403);

        var reg = await _context.FreeRegistrations.FindAsync(id);
        if (reg == null || reg.IsDeleted) return HandleError("Not found", 404);

        reg.Status = "Rejected";
        reg.RejectionReason = body?.Reason ?? "No reason provided";
        reg.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { success = true, message = "Registration rejected." });
    }

    // PUT /api/free-registrations/{id}/notes
    [HttpPut("{id}/notes")]
    [Authorize]
    public async Task<IActionResult> UpdateNotes(int id, [FromBody] NotesRequest body)
    {
        if (!IsAdmin() && !IsOwner() && !IsSalesManager())
            return HandleError("Unauthorized", 403);

        var reg = await _context.FreeRegistrations.FindAsync(id);
        if (reg == null || reg.IsDeleted) return HandleError("Not found", 404);

        reg.Notes = body.Notes;
        reg.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }

    // DELETE /api/free-registrations/{id}
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsAdmin() && !IsOwner())
            return HandleError("Unauthorized", 403);

        var reg = await _context.FreeRegistrations.FindAsync(id);
        if (reg == null || reg.IsDeleted) return HandleError("Not found", 404);

        reg.IsDeleted = true;
        reg.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { success = true, message = "Deleted." });
    }

    // GET /api/free-registrations/stats
    [HttpGet("stats")]
    [Authorize]
    public async Task<IActionResult> Stats()
    {
        if (!IsAdmin() && !IsOwner() && !IsSalesManager())
            return HandleError("Unauthorized", 403);

        var stats = new {
            total   = await _context.FreeRegistrations.CountAsync(r => !r.IsDeleted),
            pending  = await _context.FreeRegistrations.CountAsync(r => !r.IsDeleted && r.Status == "Pending"),
            approved = await _context.FreeRegistrations.CountAsync(r => !r.IsDeleted && r.Status == "Approved"),
            rejected = await _context.FreeRegistrations.CountAsync(r => !r.IsDeleted && r.Status == "Rejected"),
        };
        return Ok(new { success = true, data = stats });
    }

    private string? GetCurrentUserName()
    {
        return User?.Identity?.Name ?? User?.FindFirst("name")?.Value;
    }

    public record ApproveRequest(string? Notes);
    public record RejectRequest(string? Reason);
    public record NotesRequest(string? Notes);
}
