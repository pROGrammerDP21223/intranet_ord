using backend_net.Data.Context;
using backend_net.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Services;

/// <summary>
/// Service to update permissions in existing databases to match current modules
/// Run this once to migrate from old permissions to new permissions
/// </summary>
public static class PermissionUpdateService
{
    public static async System.Threading.Tasks.Task UpdatePermissionsToMatchModulesAsync(ApplicationDbContext context)
    {
        // Get all existing permissions
        var existingPermissions = await context.Permissions.ToListAsync();
        var existingPermissionNames = existingPermissions.Select(p => p.Name).ToList();

        var newPermissions = new List<Permission>();

        // Add new permissions that don't exist
        var requiredPermissions = new Dictionary<string, (string Description, string Category)>
        {
            // Dashboard
            { PermissionNames.ViewDashboard, ("View Dashboard", "dashboard") },
            
            // Clients
            { PermissionNames.ViewClients, ("View Clients", "clients") },
            { PermissionNames.CreateClients, ("Create Clients", "clients") },
            { PermissionNames.EditClients, ("Edit Clients", "clients") },
            { PermissionNames.DeleteClients, ("Delete Clients", "clients") },
            
            // Services
            { PermissionNames.ViewServices, ("View Services", "services") },
            { PermissionNames.CreateServices, ("Create Services", "services") },
            { PermissionNames.EditServices, ("Edit Services", "services") },
            { PermissionNames.DeleteServices, ("Delete Services", "services") },
            
            // Roles
            { PermissionNames.ViewRoles, ("View Roles", "roles") },
            { PermissionNames.CreateRoles, ("Create Roles", "roles") },
            { PermissionNames.EditRoles, ("Edit Roles", "roles") },
            { PermissionNames.DeleteRoles, ("Delete Roles", "roles") },
            
            // Permissions
            { PermissionNames.ViewPermissions, ("View Permissions", "permissions") },
            { PermissionNames.ManagePermissions, ("Manage Permissions", "permissions") },
            
            // Admin
            { PermissionNames.ManageRoles, ("Manage Roles (Full Access)", "admin") },
            { PermissionNames.ManageSettings, ("Manage Settings", "admin") },
            { PermissionNames.FullAccess, ("Full Access", "admin") }
        };

        // Add missing permissions
        foreach (var (name, (description, category)) in requiredPermissions)
        {
            if (!existingPermissionNames.Contains(name))
            {
                newPermissions.Add(new Permission
                {
                    Name = name,
                    Description = description,
                    Category = category,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        if (newPermissions.Any())
        {
            await context.Permissions.AddRangeAsync(newPermissions);
            await context.SaveChangesAsync();
            Console.WriteLine($"[PermissionUpdate] Added {newPermissions.Count} new permissions");
        }

        // Soft delete old permissions that are no longer needed
        var oldPermissionNames = new[]
        {
            "view:users", "create:users", "edit:users", "delete:users",
            "view:sales", "create:sales", "edit:sales", "delete:sales",
            "view:reports", "edit:reports"
        };

        var oldPermissions = existingPermissions
            .Where(p => oldPermissionNames.Contains(p.Name) && !p.IsDeleted)
            .ToList();

        if (oldPermissions.Any())
        {
            foreach (var oldPerm in oldPermissions)
            {
                oldPerm.IsDeleted = true;
                oldPerm.UpdatedAt = DateTime.UtcNow;
            }
            await context.SaveChangesAsync();
            Console.WriteLine($"[PermissionUpdate] Soft deleted {oldPermissions.Count} old permissions");
        }

        // Update category/description for existing permissions if needed
        foreach (var perm in existingPermissions.Where(p => !p.IsDeleted))
        {
            if (requiredPermissions.ContainsKey(perm.Name))
            {
                var (description, category) = requiredPermissions[perm.Name];
                if (perm.Description != description || perm.Category != category)
                {
                    perm.Description = description;
                    perm.Category = category;
                    perm.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine("[PermissionUpdate] Permissions updated successfully");

        // Ensure role-permission assignments are up to date
        await EnsureRolePermissionsAsync(context);
    }

    private static async System.Threading.Tasks.Task EnsureRolePermissionsAsync(ApplicationDbContext context)
    {
        var roles = await context.Roles.ToListAsync();
        var permissions = await context.Permissions.Where(p => !p.IsDeleted).ToListAsync();
        // Load all rows including soft-deleted to avoid unique constraint violations on insert
        var existingRolePermissions = await context.RolePermissions.IgnoreQueryFilters().ToListAsync();

        // Define the expected role → permission assignments
        var rolePermissionMap = new Dictionary<string, string[]>
        {
            ["Admin"] = permissions.Select(p => p.Name).ToArray(),
            ["Owner"] = permissions.Select(p => p.Name).ToArray(),
            ["HOD"] = new[]
            {
                PermissionNames.ViewDashboard, PermissionNames.ViewClients, PermissionNames.ViewServices,
                PermissionNames.EditClients, PermissionNames.EditServices,
                PermissionNames.CreateClients, PermissionNames.CreateServices
            },
            ["Sales Manager"] = new[]
            {
                PermissionNames.ViewDashboard, PermissionNames.ViewClients, PermissionNames.ViewServices,
                PermissionNames.EditClients, PermissionNames.EditServices,
                PermissionNames.CreateClients, PermissionNames.CreateServices,
                PermissionNames.DeleteClients
            },
            ["Sales Person"] = new[]
            {
                PermissionNames.ViewDashboard, PermissionNames.ViewClients, PermissionNames.ViewServices,
                PermissionNames.EditClients, PermissionNames.CreateClients
            },
            ["Employee"] = new[]
            {
                PermissionNames.ViewDashboard, PermissionNames.ViewClients, PermissionNames.ViewServices
            },
            ["Calling Staff"] = new[]
            {
                PermissionNames.ViewDashboard, PermissionNames.ViewClients,
                PermissionNames.EditClients, PermissionNames.CreateClients
            },
            ["Client"] = new[] { PermissionNames.ViewDashboard }
        };

        var toAdd = new List<RolePermission>();
        var toRestore = new List<RolePermission>();

        foreach (var (roleName, permNames) in rolePermissionMap)
        {
            var role = roles.FirstOrDefault(r => r.Name == roleName);
            if (role == null) continue;

            foreach (var permName in permNames)
            {
                var perm = permissions.FirstOrDefault(p => p.Name == permName);
                if (perm == null) continue;

                var existing = existingRolePermissions.FirstOrDefault(rp => rp.RoleId == role.Id && rp.PermissionId == perm.Id);
                if (existing == null)
                {
                    toAdd.Add(new RolePermission { RoleId = role.Id, PermissionId = perm.Id });
                }
                else if (existing.IsDeleted)
                {
                    existing.IsDeleted = false;
                    existing.UpdatedAt = DateTime.UtcNow;
                    toRestore.Add(existing);
                }
            }
        }

        if (toAdd.Any())
        {
            await context.RolePermissions.AddRangeAsync(toAdd);
        }

        if (toAdd.Any() || toRestore.Any())
        {
            await context.SaveChangesAsync();
            Console.WriteLine($"[PermissionUpdate] Added {toAdd.Count} new + restored {toRestore.Count} role-permission assignments");
        }
    }
}

