using backend_net.Data.Context;
using backend_net.Data.Interfaces;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Services;

public class ServiceService : IServiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;
    private readonly ICacheService? _cacheService;

    public ServiceService(
        IUnitOfWork unitOfWork, 
        ApplicationDbContext context,
        ICacheService? cacheService = null)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _cacheService = cacheService;
    }

    public async System.Threading.Tasks.Task<IEnumerable<Service>> GetAllAsync(bool includeInactive = false)
    {
        var cacheKey = $"services_all_{includeInactive}";
        
        // Try to get from cache
        if (_cacheService != null)
        {
            var cached = await _cacheService.GetAsync<List<Service>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }
        }

        var query = _context.Services.AsQueryable();
        
        if (!includeInactive)
        {
            query = query.Where(s => s.IsActive);
        }

        var services = await query
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Category)
            .ThenBy(s => s.ServiceName)
            .ToListAsync();

        // Cache for 1 hour
        if (_cacheService != null)
        {
            await _cacheService.SetAsync(cacheKey, services, TimeSpan.FromHours(1));
        }

        return services;
    }

    public async System.Threading.Tasks.Task<IEnumerable<object>> GetByCategoryAsync()
    {
        const string cacheKey = "services_by_category";
        
        // Try to get from cache
        if (_cacheService != null)
        {
            var cached = await _cacheService.GetAsync<List<object>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }
        }

        var result = await _context.Services
            .Where(s => s.IsActive)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.ServiceName)
            .GroupBy(s => s.Category)
            .Select(g => new
            {
                Category = g.Key,
                Services = g.Select(s => new
                {
                    s.Id,
                    s.ServiceType,
                    s.ServiceName,
                    s.Category,
                    s.SortOrder
                }).ToList()
            })
            .ToListAsync();

        // Cache for 1 hour
        if (_cacheService != null)
        {
            await _cacheService.SetAsync(cacheKey, result.Cast<object>().ToList(), TimeSpan.FromHours(1));
        }

        return result.Cast<object>();
    }

    public async System.Threading.Tasks.Task<Service?> GetByIdAsync(int id)
    {
        return await _context.Services
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async System.Threading.Tasks.Task<Service> CreateAsync(CreateServiceRequest request)
    {
        var service = new Service
        {
            ServiceType = request.ServiceType,
            ServiceName = request.ServiceName,
            Category = request.Category,
            IsActive = request.IsActive ?? true,
            SortOrder = request.SortOrder
        };

        await _context.Services.AddAsync(service);
        await _context.SaveChangesAsync();

        // Invalidate cache
        if (_cacheService != null)
        {
            await _cacheService.RemoveByPatternAsync("services_");
        }

        return service;
    }

    public async System.Threading.Tasks.Task<Service> UpdateAsync(int id, UpdateServiceRequest request)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null || service.IsDeleted)
        {
            throw new KeyNotFoundException("Service not found");
        }

        // Check if service type already exists for another service
        if (request.ServiceType != service.ServiceType)
        {
            var existingService = await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceType == request.ServiceType && s.Id != id && !s.IsDeleted);

            if (existingService != null)
            {
                throw new InvalidOperationException("Service with this type already exists");
            }
        }

        service.ServiceType = request.ServiceType;
        service.ServiceName = request.ServiceName;
        service.Category = request.Category;
        service.IsActive = request.IsActive ?? service.IsActive;
        service.SortOrder = request.SortOrder;
        service.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Invalidate cache
        if (_cacheService != null)
        {
            await _cacheService.RemoveByPatternAsync("services_");
        }

        return service;
    }

    public async System.Threading.Tasks.Task DeleteAsync(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null || service.IsDeleted)
        {
            throw new KeyNotFoundException("Service not found");
        }

        // Check if service is in use
        var isInUse = await _context.ClientServices
            .AnyAsync(cs => cs.ServiceId == id && !cs.IsDeleted);

        if (isInUse)
        {
            // Soft delete
            service.IsDeleted = true;
            service.IsActive = false;
            service.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Hard delete
            _context.Services.Remove(service);
        }

        await _context.SaveChangesAsync();

        // Invalidate cache
        if (_cacheService != null)
        {
            await _cacheService.RemoveByPatternAsync("services_");
        }
    }
}

