using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using backend_net.Domain.Entities;

namespace backend_net.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _allowedRoles;

    public AuthorizeRoleAttribute(params string[] allowedRoles)
    {
        _allowedRoles = allowedRoles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userRole) || !_allowedRoles.Contains(userRole))
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _requiredPermissions;

    public RequirePermissionAttribute(params string[] requiredPermissions)
    {
        _requiredPermissions = requiredPermissions;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Get user info for logging
        var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userEmail = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
        var logger = context.HttpContext.RequestServices.GetService(typeof(ILogger<RequirePermissionAttribute>)) as ILogger<RequirePermissionAttribute>;

        // Admin role has full access - bypass all permission checks
        if (!string.IsNullOrEmpty(userRole) && userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            logger?.LogInformation($"[AUTH] Access GRANTED for Admin user {userEmail} (ID: {userId}). Admin bypass enabled.");
            return;
        }

        // For non-admin users, check permissions
        var permissionsClaim = user.FindFirst("Permissions")?.Value;
        var permissions = permissionsClaim?.Split(',') ?? Array.Empty<string>();

        logger?.LogWarning($"[AUTH] User {userEmail} (ID: {userId}, Role: {userRole}) attempting to access endpoint requiring [{string.Join(", ", _requiredPermissions)}]. " +
                          $"User has permissions: [{(string.IsNullOrEmpty(permissionsClaim) ? "NONE" : permissionsClaim)}]");

        // Check if user has all required permissions or full access permission
        var hasAllPermissions = _requiredPermissions.All(perm =>
            permissions.Contains(perm.Trim()) ||
            permissions.Contains(PermissionNames.FullAccess));

        if (!hasAllPermissions)
        {
            logger?.LogWarning($"[AUTH] Access DENIED for user {userEmail}. Missing permissions.");
            context.Result = new ForbidResult();
            return;
        }

        logger?.LogInformation($"[AUTH] Access GRANTED for user {userEmail}.");
    }
}

