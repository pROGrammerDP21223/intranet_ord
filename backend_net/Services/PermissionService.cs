using backend_net.Data.Context;
using backend_net.Data.Interfaces;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Services;

public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public PermissionService(IUnitOfWork unitOfWork, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        return await _context.Permissions
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<object>> GetByCategoryAsync()
    {
        return await _context.Permissions
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .GroupBy(p => p.Category)
            .Select(g => new
            {
                Category = g.Key,
                Permissions = g.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<Permission?> GetByIdAsync(int id)
    {
        return await _context.Permissions
            .Where(p => p.Id == id && !p.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<Permission> CreateAsync(CreatePermissionRequest request)
    {
        // Check if permission name already exists
        if (await PermissionNameExistsAsync(request.Name))
        {
            throw new InvalidOperationException($"Permission with name '{request.Name}' already exists.");
        }

        var permission = new Permission
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Permission>().AddAsync(permission);
        await _unitOfWork.SaveChangesAsync();
        return permission;
    }

    public async Task<Permission> UpdateAsync(int id, UpdatePermissionRequest request)
    {
        var permission = await _unitOfWork.Repository<Permission>().GetByIdAsync(id);
        if (permission == null)
        {
            throw new KeyNotFoundException($"Permission with ID {id} not found");
        }

        // Check if permission name is being changed and already exists
        if (permission.Name != request.Name && await PermissionNameExistsAsync(request.Name, id))
        {
            throw new InvalidOperationException($"Permission with name '{request.Name}' already exists.");
        }

        permission.Name = request.Name;
        permission.Description = request.Description;
        permission.Category = request.Category;
        permission.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<Permission>().UpdateAsync(permission);
        await _unitOfWork.SaveChangesAsync();
        return permission;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var permission = await _unitOfWork.Repository<Permission>().GetByIdAsync(id);
        if (permission == null)
        {
            return false;
        }

        // Check if permission is being used by any roles
        var rolesUsingPermission = await _context.RolePermissions
            .Where(rp => rp.PermissionId == id && !rp.IsDeleted)
            .AnyAsync();

        if (rolesUsingPermission)
        {
            throw new InvalidOperationException("Cannot delete permission as it is assigned to existing roles.");
        }

        permission.IsDeleted = true;
        permission.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<Permission>().UpdateAsync(permission);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PermissionNameExistsAsync(string name, int? excludePermissionId = null)
    {
        var query = _context.Permissions
            .Where(p => p.Name == name && !p.IsDeleted);

        if (excludePermissionId.HasValue)
        {
            query = query.Where(p => p.Id != excludePermissionId.Value);
        }

        return await query.AnyAsync();
    }
}

