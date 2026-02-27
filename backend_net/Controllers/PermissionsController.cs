using backend_net.Controllers;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[Authorize]
public class PermissionsController : BaseController
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPermissions()
    {
        try
        {
            // Only Admin/Owner can view permissions
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view permissions", 403);
            }

            var permissions = await _permissionService.GetAllAsync();
            var permissionsDto = permissions.Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Category,
                p.CreatedAt
            }).ToList();

            return HandleSuccess("Permissions retrieved successfully", permissionsDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("by-category")]
    public async Task<IActionResult> GetPermissionsByCategory()
    {
        try
        {
            // Only Admin/Owner can view permissions
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view permissions", 403);
            }

            var permissionsByCategory = await _permissionService.GetByCategoryAsync();
            return HandleSuccess("Permissions by category retrieved successfully", permissionsByCategory);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPermissionById(int id)
    {
        try
        {
            // Only Admin/Owner can view permissions
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view permissions", 403);
            }

            var permission = await _permissionService.GetByIdAsync(id);

            if (permission == null)
            {
                return HandleError("Permission not found", 404);
            }

            var permissionDto = new
            {
                permission.Id,
                permission.Name,
                permission.Description,
                permission.Category,
                permission.CreatedAt
            };

            return HandleSuccess("Permission retrieved successfully", permissionDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Create a new permission
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionRequest request)
    {
        try
        {
            // Only Admin/Owner can create permissions
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can create permissions", 403);
            }

            var permission = await _permissionService.CreateAsync(request);
            var permissionDto = new
            {
                permission.Id,
                permission.Name,
                permission.Description,
                permission.Category,
                permission.CreatedAt
            };

            return StatusCode(201, new { message = "Permission created successfully", data = permissionDto, success = true });
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

    /// <summary>
    /// Update an existing permission
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePermission(int id, [FromBody] UpdatePermissionRequest request)
    {
        try
        {
            // Only Admin/Owner can update permissions
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can update permissions", 403);
            }

            var permission = await _permissionService.UpdateAsync(id, request);
            var permissionDto = new
            {
                permission.Id,
                permission.Name,
                permission.Description,
                permission.Category,
                permission.UpdatedAt
            };

            return HandleSuccess("Permission updated successfully", permissionDto);
        }
        catch (KeyNotFoundException ex)
        {
            return HandleError(ex.Message, 404);
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

    /// <summary>
    /// Delete a permission (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePermission(int id)
    {
        try
        {
            // Only Admin/Owner can delete permissions
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can delete permissions", 403);
            }

            var result = await _permissionService.DeleteAsync(id);
            if (!result)
            {
                return HandleError("Permission not found", 404);
            }

            return HandleSuccess("Permission deleted successfully");
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

