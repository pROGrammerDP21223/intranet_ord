using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class Permission : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string Category { get; set; } = string.Empty; // e.g., "dashboard", "users", "sales", "reports"

    // Navigation property for many-to-many relationship
    [NotMapped]
    public ICollection<Role>? Roles { get; set; }
}

// Permission constants for seeding
public static class PermissionNames
{
    // Dashboard permissions
    public const string ViewDashboard = "view:dashboard";
    
    // Client permissions (Client Management Module)
    public const string ViewClients = "view:clients";
    public const string CreateClients = "create:clients";
    public const string EditClients = "edit:clients";
    public const string DeleteClients = "delete:clients";
    
    // Services permissions (Services Management Module)
    public const string ViewServices = "view:services";
    public const string CreateServices = "create:services";
    public const string EditServices = "edit:services";
    public const string DeleteServices = "delete:services";
    
    // Roles & Permissions permissions (Permissions Management Module)
    public const string ViewRoles = "view:roles";
    public const string CreateRoles = "create:roles";
    public const string EditRoles = "edit:roles";
    public const string DeleteRoles = "delete:roles";
    public const string ViewPermissions = "view:permissions";
    public const string ManagePermissions = "manage:permissions"; // Assign permissions to roles
    
    // Admin permissions
    public const string ManageRoles = "manage:roles"; // Full role management (includes all role operations)
    public const string ManageSettings = "manage:settings";
    public const string FullAccess = "full:access";
}

