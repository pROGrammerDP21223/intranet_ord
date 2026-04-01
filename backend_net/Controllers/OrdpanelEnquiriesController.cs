using backend_net.Data.Context;
using backend_net.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Controllers;

[Route("api/ordpanel-enquiries")]
[ApiController]
public class OrdpanelEnquiriesController : BaseController
{
    private readonly ApplicationDbContext _context;

    public OrdpanelEnquiriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /api/ordpanel-enquiries  (Admin, Owner only)
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status = null,
        [FromQuery] string? pageType = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        if (!IsAdmin() && !IsOwner())
            return HandleError("Unauthorized: Only Admin and Owner can view Ordpanel Enquiries", 403);

        var query = _context.OrdpanelEnquiries
            .Where(e => !e.IsDeleted)
            .OrderByDescending(e => e.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))   query = query.Where(e => e.Status == status);
        if (!string.IsNullOrEmpty(pageType)) query = query.Where(e => e.PageType == pageType);
        if (from.HasValue) query = query.Where(e => e.CreatedAt >= from.Value);
        if (to.HasValue)   query = query.Where(e => e.CreatedAt <= to.Value.AddDays(1));

        var list = await query.Select(e => new {
            e.Id, e.Name, e.Email, e.Phone,
            e.ProductName, e.ClientName, e.ListingClientId, e.Message,
            e.PageType, e.PageUrl, e.Status,
            e.CreatedAt, e.UpdatedAt
        }).ToListAsync();

        return Ok(new { success = true, data = list });
    }

    // GET /api/ordpanel-enquiries/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        if (!IsAdmin() && !IsOwner())
            return HandleError("Unauthorized", 403);

        var enq = await _context.OrdpanelEnquiries
            .Where(e => e.Id == id && !e.IsDeleted)
            .Select(e => new {
                e.Id, e.Name, e.Email, e.Phone,
                e.ProductName, e.ClientName, e.ListingClientId, e.Message,
                e.PageType, e.PageUrl, e.Status,
                e.CreatedAt, e.UpdatedAt
            }).FirstOrDefaultAsync();

        if (enq == null) return HandleError("Not found", 404);
        return Ok(new { success = true, data = enq });
    }

    // PUT /api/ordpanel-enquiries/{id}/status
    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusRequest body)
    {
        if (!IsAdmin() && !IsOwner())
            return HandleError("Unauthorized", 403);

        var enq = await _context.OrdpanelEnquiries.FindAsync(id);
        if (enq == null || enq.IsDeleted) return HandleError("Not found", 404);

        var valid = new[] { "New", "Read", "Responded" };
        if (!valid.Contains(body.Status))
            return HandleError("Invalid status value", 400);

        enq.Status = body.Status;
        enq.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }

    // DELETE /api/ordpanel-enquiries/{id}
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsAdmin() && !IsOwner())
            return HandleError("Unauthorized", 403);

        var enq = await _context.OrdpanelEnquiries.FindAsync(id);
        if (enq == null || enq.IsDeleted) return HandleError("Not found", 404);

        enq.IsDeleted = true;
        enq.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { success = true, message = "Deleted." });
    }

    // GET /api/ordpanel-enquiries/stats
    [HttpGet("stats")]
    [Authorize]
    public async Task<IActionResult> Stats()
    {
        if (!IsAdmin() && !IsOwner())
            return HandleError("Unauthorized", 403);

        var stats = new {
            total     = await _context.OrdpanelEnquiries.CountAsync(e => !e.IsDeleted),
            newCount  = await _context.OrdpanelEnquiries.CountAsync(e => !e.IsDeleted && e.Status == "New"),
            read      = await _context.OrdpanelEnquiries.CountAsync(e => !e.IsDeleted && e.Status == "Read"),
            responded = await _context.OrdpanelEnquiries.CountAsync(e => !e.IsDeleted && e.Status == "Responded"),
        };
        return Ok(new { success = true, data = stats });
    }

    public record StatusRequest(string Status);
}
