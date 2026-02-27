using backend_net.Controllers;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[Authorize]
public class RolesController : BaseController
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRoles()
    {
        try
        {
            // Only Admin/Owner can view roles
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view roles", 403);
            }

            var roles = await _roleService.GetAllAsync();
            var rolesDto = roles.Select(r => 
            {
                // Debug: Check what we have
                var rolePerms = r.RolePermissions ?? new List<RolePermission>();
                var validPerms = rolePerms
                    .Where(rp => rp != null && !rp.IsDeleted && rp.Permission != null && !rp.Permission.IsDeleted)
                    .Select(rp => new
                    {
                        rp.Permission!.Id,
                        rp.Permission.Name,
                        rp.Permission.Description,
                        rp.Permission.Category
                    })
                    .ToList();

                return new
                {
                    r.Id,
                    r.Name,
                    r.Description,
                    Permissions = validPerms.Cast<object>().ToList()
                };
            }).ToList();

            return HandleSuccess("Roles retrieved successfully", rolesDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoleById(int id)
    {
        try
        {
            // Only Admin/Owner can view roles
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view roles", 403);
            }

            var role = await _roleService.GetByIdAsync(id);

            if (role == null)
            {
                return HandleError("Role not found", 404);
            }

            var roleDto = new
            {
                role.Id,
                role.Name,
                role.Description,
                Permissions = role.RolePermissions != null && role.RolePermissions.Any()
                    ? role.RolePermissions
                        .Where(rp => rp.Permission != null && !rp.IsDeleted && !rp.Permission.IsDeleted)
                        .Select(rp => new
                        {
                            rp.Permission!.Id,
                            rp.Permission.Name,
                            rp.Permission.Description,
                            rp.Permission.Category
                        })
                        .Cast<object>()
                        .ToList()
                    : new List<object>()
            };

            return HandleSuccess("Role retrieved successfully", roleDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> GetAllPermissions()
    {
        try
        {
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view permissions", 403);
            }

            var permissions = await _roleService.GetAllPermissionsAsync();
            var permissionsDto = permissions.Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Category
            }).ToList();

            return HandleSuccess("Permissions retrieved successfully", permissionsDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        try
        {
            // Only Admin/Owner can create roles
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can create roles", 403);
            }

            if (!ModelState.IsValid)
            {
                return HandleError("Invalid request data", 400);
            }

            var role = await _roleService.CreateAsync(request.Name, request.Description);
            
            var roleDto = new
            {
                role.Id,
                role.Name,
                role.Description,
                Permissions = new List<object>()
            };

            return HandleSuccess("Role created successfully", roleDto);
        }
        catch (InvalidOperationException ex)
        {
            return HandleError(ex.Message, 400);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleRequest request)
    {
        try
        {
            // Only Admin/Owner can update roles
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can update roles", 403);
            }

            if (!ModelState.IsValid)
            {
                return HandleError("Invalid request data", 400);
            }

            var role = await _roleService.UpdateAsync(id, request.Name, request.Description);
            
            // Reload role with permissions for response
            var updatedRole = await _roleService.GetByIdAsync(id);
            
            var roleDto = new
            {
                role.Id,
                role.Name,
                role.Description,
                Permissions = updatedRole?.RolePermissions != null && updatedRole.RolePermissions.Any()
                    ? updatedRole.RolePermissions
                        .Where(rp => rp.Permission != null && !rp.IsDeleted && !rp.Permission.IsDeleted)
                        .Select(rp => new
                        {
                            rp.Permission!.Id,
                            rp.Permission.Name,
                            rp.Permission.Description,
                            rp.Permission.Category
                        })
                        .Cast<object>()
                        .ToList()
                    : new List<object>()
            };

            return HandleSuccess("Role updated successfully", roleDto);
        }
        catch (KeyNotFoundException)
        {
            return HandleError("Role not found", 404);
        }
        catch (InvalidOperationException ex)
        {
            return HandleError(ex.Message, 400);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        try
        {
            // Only Admin/Owner can delete roles
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can delete roles", 403);
            }

            var result = await _roleService.DeleteAsync(id);
            if (!result)
            {
                return HandleError("Role not found", 404);
            }

            return HandleSuccess("Role deleted successfully", null);
        }
        catch (KeyNotFoundException)
        {
            return HandleError("Role not found", 404);
        }
        catch (InvalidOperationException ex)
        {
            return HandleError(ex.Message, 400);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpPost("{id}/permissions")]
    public async Task<IActionResult> AssignPermissions(int id, [FromBody] AssignPermissionsRequest request)
    {
        try
        {
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can assign permissions", 403);
            }

            if (!ModelState.IsValid)
            {
                return HandleError("Invalid request data", 400);
            }

            var updatedRole = await _roleService.AssignPermissionsAsync(id, request.PermissionIds);
            var roleDto = new
            {
                updatedRole.Id,
                updatedRole.Name,
                updatedRole.Description,
                Permissions = updatedRole.RolePermissions != null && updatedRole.RolePermissions.Any()
                    ? updatedRole.RolePermissions
                        .Where(rp => rp.Permission != null && !rp.IsDeleted && !rp.Permission.IsDeleted)
                        .Select(rp => new
                        {
                            rp.Permission!.Id,
                            rp.Permission.Name,
                            rp.Permission.Description,
                            rp.Permission.Category
                        })
                        .Cast<object>()
                        .ToList()
                    : new List<object>()
            };

            return HandleSuccess("Permissions assigned successfully", roleDto);
        }
        catch (KeyNotFoundException)
        {
            return HandleError("Role not found", 404);
        }
        catch (InvalidOperationException ex)
        {
            return HandleError(ex.Message, 400);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }
}

