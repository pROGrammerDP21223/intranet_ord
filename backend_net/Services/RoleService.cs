using backend_net.Data.Context;
using backend_net.Data.Interfaces;
using backend_net.Domain.Entities;
using backend_net.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Services;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public RoleService(IUnitOfWork unitOfWork, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        // Load roles (ignoring query filter on RolePermissions to load them manually)
        var roles = await _context.Roles
            .Where(r => !r.IsDeleted)
            .ToListAsync();

        // Load all RolePermissions for these roles (ignoring global query filter)
        var roleIds = roles.Select(r => r.Id).ToList();
        var allRolePermissions = await _context.RolePermissions
            .IgnoreQueryFilters()
            .Where(rp => roleIds.Contains(rp.RoleId) && !rp.IsDeleted)
            .ToListAsync();

        // Get all unique permission IDs
        var allPermissionIds = allRolePermissions
            .Select(rp => rp.PermissionId)
            .Distinct()
            .ToList();

        // Load all permissions (ignoring global query filter)
        var allPermissions = await _context.Permissions
            .IgnoreQueryFilters()
            .Where(p => allPermissionIds.Contains(p.Id))
            .ToListAsync();

        // Attach RolePermissions to Roles and Permissions to RolePermissions
        foreach (var role in roles)
        {
            var rolePerms = allRolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .ToList();

            // Create a new collection to avoid EF Core tracking issues
            role.RolePermissions = new List<RolePermission>();

            foreach (var rp in rolePerms)
            {
                var permission = allPermissions.FirstOrDefault(p => p.Id == rp.PermissionId);
                if (permission != null && !permission.IsDeleted)
                {
                    // Create a new RolePermission object with the permission attached
                    var rolePermission = new RolePermission
                    {
                        Id = rp.Id,
                        RoleId = rp.RoleId,
                        PermissionId = rp.PermissionId,
                        IsDeleted = rp.IsDeleted,
                        CreatedAt = rp.CreatedAt,
                        UpdatedAt = rp.UpdatedAt,
                        Permission = permission,
                        Role = role
                    };
                    role.RolePermissions.Add(rolePermission);
                }
            }
        }

        return roles;
    }

    public async Task<Role?> GetByIdAsync(int id)
    {
        var role = await _context.Roles
            .Where(r => r.Id == id && !r.IsDeleted)
            .FirstOrDefaultAsync();

        if (role == null) return null;

        // Load RolePermissions for this role (ignoring global query filter)
        var rolePermissions = await _context.RolePermissions
            .IgnoreQueryFilters()
            .Where(rp => rp.RoleId == id && !rp.IsDeleted)
            .ToListAsync();

        // Get permission IDs
        var permissionIds = rolePermissions.Select(rp => rp.PermissionId).ToList();

        // Load permissions (ignoring global query filter)
        var permissions = await _context.Permissions
            .IgnoreQueryFilters()
            .Where(p => permissionIds.Contains(p.Id))
            .ToListAsync();

        // Create new collection with properly attached permissions
        role.RolePermissions = new List<RolePermission>();
        foreach (var rp in rolePermissions)
        {
            var permission = permissions.FirstOrDefault(p => p.Id == rp.PermissionId);
            if (permission != null && !permission.IsDeleted)
            {
                var rolePermission = new RolePermission
                {
                    Id = rp.Id,
                    RoleId = rp.RoleId,
                    PermissionId = rp.PermissionId,
                    IsDeleted = rp.IsDeleted,
                    CreatedAt = rp.CreatedAt,
                    UpdatedAt = rp.UpdatedAt,
                    Permission = permission,
                    Role = role
                };
                role.RolePermissions.Add(rolePermission);
            }
        }

        return role;
    }

    public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
    {
        return await _context.Permissions
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Role> CreateAsync(string name, string? description)
    {
        // Check if role with same name already exists
        var existingRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !r.IsDeleted);
        
        if (existingRole != null)
        {
            throw new InvalidOperationException($"Role with name '{name}' already exists");
        }

        var role = new Role
        {
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Roles.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();
        return role;
    }

    public async Task<Role> UpdateAsync(int id, string name, string? description)
    {
        var role = await GetByIdAsync(id);
        if (role == null)
        {
            throw new KeyNotFoundException($"Role with ID {id} not found");
        }

        // Check if role name is being changed and already exists
        if (!role.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            var existingRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase) 
                    && !r.IsDeleted 
                    && r.Id != id);
            
            if (existingRole != null)
            {
                throw new InvalidOperationException($"Role with name '{name}' already exists");
            }
        }

        role.Name = name;
        role.Description = description;
        role.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return role;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var role = await GetByIdAsync(id);
        if (role == null)
        {
            return false;
        }

        // Check if role is being used by any users
        var usersWithRole = await _context.Users
            .Where(u => u.RoleId == id && !u.IsDeleted)
            .AnyAsync();
        
        if (usersWithRole)
        {
            throw new InvalidOperationException("Cannot delete role that is assigned to users");
        }

        role.IsDeleted = true;
        role.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<Role> AssignPermissionsAsync(int roleId, List<int> permissionIds)
    {
        var role = await GetByIdAsync(roleId);
        if (role == null)
        {
            throw new KeyNotFoundException($"Role with ID {roleId} not found");
        }

        // Validate that all permission IDs exist
        var validPermissionIds = await _context.Permissions
            .Where(p => permissionIds.Contains(p.Id) && !p.IsDeleted)
            .Select(p => p.Id)
            .ToListAsync();

        var invalidIds = permissionIds.Except(validPermissionIds).ToList();
        if (invalidIds.Any())
        {
            throw new InvalidOperationException($"Invalid permission IDs: {string.Join(", ", invalidIds)}");
        }

        // Use execution strategy for transaction to support SQL Server retry logic
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get ALL RolePermissions for this role (including soft-deleted) to avoid duplicates
                var allExistingPermissions = await _context.RolePermissions
                    .IgnoreQueryFilters()
                    .Where(rp => rp.RoleId == roleId)
                    .ToListAsync();

                // Get currently active permissions
                var activePermissions = allExistingPermissions.Where(rp => !rp.IsDeleted).ToList();
                
                // Soft delete all currently active permissions
                foreach (var existing in activePermissions)
                {
                    existing.IsDeleted = true;
                    existing.UpdatedAt = DateTime.UtcNow;
                }

                // Get all existing permission IDs (both active and deleted)
                var allExistingPermissionIds = allExistingPermissions.Select(ep => ep.PermissionId).ToList();
                
                // Find permissions that need to be created (don't exist at all)
                var newPermissionIds = permissionIds.Except(allExistingPermissionIds).ToList();

                // Create new RolePermissions for permissions that don't exist
                if (newPermissionIds.Any())
                {
                    var newRolePermissions = newPermissionIds.Select(permissionId => new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permissionId,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    }).ToList();

                    await _context.RolePermissions.AddRangeAsync(newRolePermissions);
                }

                // Reactivate or activate permissions that are in the new list
                // This includes both previously soft-deleted and currently active ones
                var permissionsToActivate = allExistingPermissions
                    .Where(ep => permissionIds.Contains(ep.PermissionId))
                    .ToList();

                foreach (var toActivate in permissionsToActivate)
                {
                    toActivate.IsDeleted = false;
                    toActivate.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });

        // Reload role with permissions after transaction to ensure fresh data
        // Clear change tracker to force reload from database
        _context.Entry(role).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        
        // Reload role with permissions
        var updatedRole = await GetByIdAsync(roleId);
        return updatedRole ?? role;
    }
}

