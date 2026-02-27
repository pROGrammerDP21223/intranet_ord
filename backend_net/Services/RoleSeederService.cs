using backend_net.Data.Context;
using backend_net.Domain.Entities;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Services;

public static class RoleSeederService
{
    public static async System.Threading.Tasks.Task SeedRolesAsync(ApplicationDbContext context)
    {
        // Seed Permissions first
        await SeedPermissionsAsync(context);

        // Seed Roles
        await SeedRolesDataAsync(context);

        // Assign permissions to roles
        await AssignPermissionsToRolesAsync(context);

        // Seed default admin account
        await SeedDefaultAdminAsync(context);
    }

    private static async System.Threading.Tasks.Task SeedPermissionsAsync(ApplicationDbContext context)
    {
        if (await context.Permissions.AnyAsync())
        {
            return; // Permissions already seeded
        }

        var permissions = new List<Permission>
        {
            // Dashboard permissions
            new Permission { Name = PermissionNames.ViewDashboard, Description = "View Dashboard", Category = "dashboard" },
            
            // Client permissions (Client Management Module)
            new Permission { Name = PermissionNames.ViewClients, Description = "View Clients", Category = "clients" },
            new Permission { Name = PermissionNames.CreateClients, Description = "Create Clients", Category = "clients" },
            new Permission { Name = PermissionNames.EditClients, Description = "Edit Clients", Category = "clients" },
            new Permission { Name = PermissionNames.DeleteClients, Description = "Delete Clients", Category = "clients" },
            
            // Services permissions (Services Management Module)
            new Permission { Name = PermissionNames.ViewServices, Description = "View Services", Category = "services" },
            new Permission { Name = PermissionNames.CreateServices, Description = "Create Services", Category = "services" },
            new Permission { Name = PermissionNames.EditServices, Description = "Edit Services", Category = "services" },
            new Permission { Name = PermissionNames.DeleteServices, Description = "Delete Services", Category = "services" },
            
            // Roles & Permissions permissions (Permissions Management Module)
            new Permission { Name = PermissionNames.ViewRoles, Description = "View Roles", Category = "roles" },
            new Permission { Name = PermissionNames.CreateRoles, Description = "Create Roles", Category = "roles" },
            new Permission { Name = PermissionNames.EditRoles, Description = "Edit Roles", Category = "roles" },
            new Permission { Name = PermissionNames.DeleteRoles, Description = "Delete Roles", Category = "roles" },
            new Permission { Name = PermissionNames.ViewPermissions, Description = "View Permissions", Category = "permissions" },
            new Permission { Name = PermissionNames.ManagePermissions, Description = "Manage Permissions", Category = "permissions" },
            
            // Admin permissions
            new Permission { Name = PermissionNames.ManageRoles, Description = "Manage Roles (Full Access)", Category = "admin" },
            new Permission { Name = PermissionNames.ManageSettings, Description = "Manage Settings", Category = "admin" },
            new Permission { Name = PermissionNames.FullAccess, Description = "Full Access", Category = "admin" }
        };

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();
    }

    private static async System.Threading.Tasks.Task SeedRolesDataAsync(ApplicationDbContext context)
    {
        if (await context.Roles.AnyAsync())
        {
            return; // Roles already seeded
        }

        var roles = new List<Role>
        {
            new Role { Name = "Admin", Description = "Administrator with full system access" },
            new Role { Name = "Owner", Description = "Business owner with full access" },
            new Role { Name = "HOD", Description = "Head of Department" },
            new Role { Name = "Sales Manager", Description = "Sales Manager" },
            new Role { Name = "Sales Person", Description = "Sales Person" },
            new Role { Name = "Employee", Description = "Regular Employee" },
            new Role { Name = "Calling Staff", Description = "Calling Staff" },
            new Role { Name = "Client", Description = "Client/Customer" }
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();
    }

    private static async System.Threading.Tasks.Task AssignPermissionsToRolesAsync(ApplicationDbContext context)
    {
        if (await context.RolePermissions.AnyAsync())
        {
            return; // Permissions already assigned
        }

        // Get all roles and permissions
        var roles = await context.Roles.ToListAsync();
        var permissions = await context.Permissions.ToListAsync();

        // Find roles by name dynamically
        var adminRole = roles.FirstOrDefault(r => r.Name == "Admin");
        var ownerRole = roles.FirstOrDefault(r => r.Name == "Owner");
        var hodRole = roles.FirstOrDefault(r => r.Name == "HOD");
        var salesManagerRole = roles.FirstOrDefault(r => r.Name == "Sales Manager");
        var salesPersonRole = roles.FirstOrDefault(r => r.Name == "Sales Person");
        var employeeRole = roles.FirstOrDefault(r => r.Name == "Employee");
        var callingStaffRole = roles.FirstOrDefault(r => r.Name == "Calling Staff");
        var clientRole = roles.FirstOrDefault(r => r.Name == "Client");

        var rolePermissions = new List<RolePermission>();

        // Admin and Owner get all permissions
        if (adminRole != null && ownerRole != null)
        {
            foreach (var permission in permissions)
            {
                rolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = permission.Id });
                rolePermissions.Add(new RolePermission { RoleId = ownerRole.Id, PermissionId = permission.Id });
            }
        }

        // HOD permissions
        if (hodRole != null)
        {
            var hodPermissions = new[]
            {
                PermissionNames.ViewDashboard,
                PermissionNames.ViewClients,
                PermissionNames.ViewServices,
                PermissionNames.EditClients,
                PermissionNames.EditServices,
                PermissionNames.CreateClients,
                PermissionNames.CreateServices
            };
            foreach (var permName in hodPermissions)
            {
                var perm = permissions.FirstOrDefault(p => p.Name == permName);
                if (perm != null)
                {
                    rolePermissions.Add(new RolePermission { RoleId = hodRole.Id, PermissionId = perm.Id });
                }
            }
        }

        // Sales Manager permissions
        if (salesManagerRole != null)
        {
            var salesManagerPermissions = new[]
            {
                PermissionNames.ViewDashboard,
                PermissionNames.ViewClients,
                PermissionNames.ViewServices,
                PermissionNames.EditClients,
                PermissionNames.EditServices,
                PermissionNames.CreateClients,
                PermissionNames.CreateServices,
                PermissionNames.DeleteClients
            };
            foreach (var permName in salesManagerPermissions)
            {
                var perm = permissions.FirstOrDefault(p => p.Name == permName);
                if (perm != null)
                {
                    rolePermissions.Add(new RolePermission { RoleId = salesManagerRole.Id, PermissionId = perm.Id });
                }
            }
        }

        // Sales Person permissions
        if (salesPersonRole != null)
        {
            var salesPersonPermissions = new[]
            {
                PermissionNames.ViewDashboard,
                PermissionNames.ViewClients,
                PermissionNames.ViewServices,
                PermissionNames.EditClients,
                PermissionNames.CreateClients
            };
            foreach (var permName in salesPersonPermissions)
            {
                var perm = permissions.FirstOrDefault(p => p.Name == permName);
                if (perm != null)
                {
                    rolePermissions.Add(new RolePermission { RoleId = salesPersonRole.Id, PermissionId = perm.Id });
                }
            }
        }

        // Employee permissions
        if (employeeRole != null)
        {
            var employeePermissions = new[]
            {
                PermissionNames.ViewDashboard,
                PermissionNames.ViewClients,
                PermissionNames.ViewServices
            };
            foreach (var permName in employeePermissions)
            {
                var perm = permissions.FirstOrDefault(p => p.Name == permName);
                if (perm != null)
                {
                    rolePermissions.Add(new RolePermission { RoleId = employeeRole.Id, PermissionId = perm.Id });
                }
            }
        }

        // Calling Staff permissions
        if (callingStaffRole != null)
        {
            var callingStaffPermissions = new[]
            {
                PermissionNames.ViewDashboard,
                PermissionNames.ViewClients,
                PermissionNames.EditClients,
                PermissionNames.CreateClients
            };
            foreach (var permName in callingStaffPermissions)
            {
                var perm = permissions.FirstOrDefault(p => p.Name == permName);
                if (perm != null)
                {
                    rolePermissions.Add(new RolePermission { RoleId = callingStaffRole.Id, PermissionId = perm.Id });
                }
            }
        }

        // Client permissions
        if (clientRole != null)
        {
            var clientPermissions = new[]
            {
                PermissionNames.ViewDashboard
            };
            foreach (var permName in clientPermissions)
            {
                var perm = permissions.FirstOrDefault(p => p.Name == permName);
                if (perm != null)
                {
                    rolePermissions.Add(new RolePermission { RoleId = clientRole.Id, PermissionId = perm.Id });
                }
            }
        }

        await context.RolePermissions.AddRangeAsync(rolePermissions);
        await context.SaveChangesAsync();
    }

    private static async System.Threading.Tasks.Task SeedDefaultAdminAsync(ApplicationDbContext context)
    {
        try
        {
            // Check if admin user already exists
            var adminEmail = "admin@onerankdigital.com";
            var existingAdmin = await context.Users
                .FirstOrDefaultAsync(u => u.Email == adminEmail.ToLowerInvariant() && !u.IsDeleted);

            if (existingAdmin != null)
            {
                Console.WriteLine($"[SeedDefaultAdmin] Admin user already exists: {adminEmail}");
                return; // Admin already exists
            }

            // Get Admin role
            var adminRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Admin");

            if (adminRole == null)
            {
                Console.WriteLine("[SeedDefaultAdmin] ERROR: Admin role not found. Cannot create admin user.");
                return;
            }

            Console.WriteLine($"[SeedDefaultAdmin] Creating admin user with email: {adminEmail}");

            // Create default admin user
            var adminUser = new User
            {
                Name = "Administrator",
                Email = adminEmail.ToLowerInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), // Default password - should be changed after first login
                IsActive = true,
                RoleId = adminRole.Id,
                Role = adminRole,
                CreatedBy = "System",
                UpdatedBy = "System",
                LastUpdatedBy = "System",
                CreatedAt = DateTime.UtcNow
            };

            await context.Users.AddAsync(adminUser);
            var result = await context.SaveChangesAsync();

            if (result > 0)
            {
                Console.WriteLine($"[SeedDefaultAdmin] SUCCESS: Admin user created successfully. Email: {adminEmail}, Password: Admin@123");
            }
            else
            {
                Console.WriteLine("[SeedDefaultAdmin] WARNING: No changes saved. Admin user may not have been created.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SeedDefaultAdmin] ERROR: Failed to create admin user: {ex.Message}");
            Console.WriteLine($"[SeedDefaultAdmin] Stack trace: {ex.StackTrace}");
            // Don't throw - allow application to continue even if admin creation fails
        }
    }
}
