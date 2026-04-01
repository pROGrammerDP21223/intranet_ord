using backend_net.Data.Context;
using backend_net.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Controllers;

[Route("api/contact-forms")]
[ApiController]
public class ContactFormsController : BaseController
{
    private readonly ApplicationDbContext _context;

    public ContactFormsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /api/contact-forms  (Admin, Owner only)
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null)
    {
        if (!IsAdmin() && !IsOwner())
            return HandleError("Unauthorized: Only Admin and Owner can view Contact Forms", 403);

        var query = _context.ContactForms
            .Where(e => !e.IsDeleted)
            .OrderByDescending(e => e.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(e => e.Status == status);

        var list = await query.Select(e => new {
            e.Id, e.Name, e.Email, e.Phone, e.Company,
            e.Subject, e.Message, e.Status,
            e.CreatedAt, e.UpdatedAt
        }).ToListAsync();

        return Ok(new { success = true, data = list });
    }

    // GET /api/contact-forms/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        if (!IsAdmin() && !IsOwner())
            return HandleError("Unauthorized", 403);

        var form = await _context.ContactForms
            .Where(e => e.Id == id && !e.IsDeleted)
            .Select(e => new {
                e.Id, e.Name, e.Email, e.Phone, e.Company,
                e.Subject, e.Message, e.Status,
                e.CreatedAt, e.UpdatedAt
            }).FirstOrDefaultAsync();

        if (form == null) return HandleError("Not found", 404);
        return Ok(new { success = true, data = form });
    }

    // PUT /api/contact-forms/{id}/status
    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusRequest body)
    {
        if (!IsAdmin() && !IsOwner())
            return HandleError("Unauthorized", 403);

        var form = await _context.ContactForms.FindAsync(id);
        if (form == null || form.IsDeleted) return HandleError("Not found", 404);

        var valid = new[] { "New", "Read", "Responded" };
        if (!valid.Contains(body.Status))
            return HandleError("Invalid status value", 400);

        form.Status = body.Status;
        form.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }

    // DELETE /api/contact-forms/{id}
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsAdmin() && !IsOwner())
            return HandleError("Unauthorized", 403);

        var form = await _context.ContactForms.FindAsync(id);
        if (form == null || form.IsDeleted) return HandleError("Not found", 404);

        form.IsDeleted = true;
        form.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { success = true, message = "Deleted." });
    }

    // GET /api/contact-forms/stats
    [HttpGet("stats")]
    [Authorize]
    public async Task<IActionResult> Stats()
    {
        if (!IsAdmin() && !IsOwner())
            return HandleError("Unauthorized", 403);

        var stats = new {
            total     = await _context.ContactForms.CountAsync(e => !e.IsDeleted),
            newCount  = await _context.ContactForms.CountAsync(e => !e.IsDeleted && e.Status == "New"),
            read      = await _context.ContactForms.CountAsync(e => !e.IsDeleted && e.Status == "Read"),
            responded = await _context.ContactForms.CountAsync(e => !e.IsDeleted && e.Status == "Responded"),
        };
        return Ok(new { success = true, data = stats });
    }

    public record StatusRequest(string Status);
}
