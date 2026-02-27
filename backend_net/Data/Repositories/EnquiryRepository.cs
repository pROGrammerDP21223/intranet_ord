using backend_net.Data.Context;
using backend_net.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace backend_net.Data.Repositories;

/// <summary>
/// Repository implementation for Enquiry entity
/// Follows Repository pattern - encapsulates data access logic
/// </summary>
public class EnquiryRepository : IEnquiryRepository
{
    private readonly ApplicationDbContext _context;

    public EnquiryRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async System.Threading.Tasks.Task<Enquiry?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Enquiries
            .Include(e => e.Client)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Enquiries
            .Include(e => e.Client)
            .Where(e => !e.IsDeleted)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetAllAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        var query = _context.Enquiries
            .Include(e => e.Client)
            .Where(e => !e.IsDeleted);

        if (startDate.HasValue)
        {
            query = query.Where(e => e.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(e => e.CreatedAt <= endDate.Value);
        }

        return await query
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetByClientIdAsync(int clientId, CancellationToken cancellationToken = default)
    {
        return await _context.Enquiries
            .Include(e => e.Client)
            .Where(e => !e.IsDeleted && e.ClientId == clientId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetByClientIdAsync(int clientId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        var query = _context.Enquiries
            .Include(e => e.Client)
            .Where(e => !e.IsDeleted && e.ClientId == clientId);

        if (startDate.HasValue)
        {
            query = query.Where(e => e.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(e => e.CreatedAt <= endDate.Value);
        }

        return await query
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _context.Enquiries
            .Include(e => e.Client)
            .Where(e => !e.IsDeleted && e.Status == status)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetByStatusAsync(string status, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        var query = _context.Enquiries
            .Include(e => e.Client)
            .Where(e => !e.IsDeleted && e.Status == status);

        if (startDate.HasValue)
        {
            query = query.Where(e => e.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(e => e.CreatedAt <= endDate.Value);
        }

        return await query
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetFilteredByClientIdsAsync(List<int> clientIds, CancellationToken cancellationToken = default)
    {
        return await _context.Enquiries
            .Include(e => e.Client)
            .Where(e => !e.IsDeleted && clientIds.Contains(e.ClientId))
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetFilteredByClientIdsAsync(List<int> clientIds, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        var query = _context.Enquiries
            .Include(e => e.Client)
            .Where(e => !e.IsDeleted && clientIds.Contains(e.ClientId));

        if (startDate.HasValue)
        {
            query = query.Where(e => e.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(e => e.CreatedAt <= endDate.Value);
        }

        return await query
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetByStatusFilteredByClientIdsAsync(string status, List<int> clientIds, CancellationToken cancellationToken = default)
    {
        return await _context.Enquiries
            .Include(e => e.Client)
            .Where(e => !e.IsDeleted && e.Status == status && clientIds.Contains(e.ClientId))
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetByStatusFilteredByClientIdsAsync(string status, List<int> clientIds, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        var query = _context.Enquiries
            .Include(e => e.Client)
            .Where(e => !e.IsDeleted && e.Status == status && clientIds.Contains(e.ClientId));

        if (startDate.HasValue)
        {
            query = query.Where(e => e.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(e => e.CreatedAt <= endDate.Value);
        }

        return await query
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task<int> GetCountByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _context.Enquiries
            .CountAsync(e => !e.IsDeleted && e.Status == status, cancellationToken);
    }

    public async System.Threading.Tasks.Task<int> GetCountByStatusFilteredByClientIdsAsync(string status, List<int> clientIds, CancellationToken cancellationToken = default)
    {
        return await _context.Enquiries
            .CountAsync(e => !e.IsDeleted && e.Status == status && clientIds.Contains(e.ClientId), cancellationToken);
    }

    public async System.Threading.Tasks.Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Enquiries
            .CountAsync(e => !e.IsDeleted, cancellationToken);
    }

    public async System.Threading.Tasks.Task<int> GetTotalCountFilteredByClientIdsAsync(List<int> clientIds, CancellationToken cancellationToken = default)
    {
        return await _context.Enquiries
            .CountAsync(e => !e.IsDeleted && clientIds.Contains(e.ClientId), cancellationToken);
    }

    public async System.Threading.Tasks.Task<Enquiry> AddAsync(Enquiry enquiry, CancellationToken cancellationToken = default)
    {
        await _context.Enquiries.AddAsync(enquiry, cancellationToken);
        return enquiry;
    }

    public System.Threading.Tasks.Task UpdateAsync(Enquiry enquiry, CancellationToken cancellationToken = default)
    {
        _context.Enquiries.Update(enquiry);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public System.Threading.Tasks.Task DeleteAsync(Enquiry enquiry, CancellationToken cancellationToken = default)
    {
        enquiry.IsDeleted = true;
        enquiry.UpdatedAt = DateTime.UtcNow;
        _context.Enquiries.Update(enquiry);
        return System.Threading.Tasks.Task.CompletedTask;
    }
}

