using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected IActionResult HandleResult<T>(T? result)
    {
        if (result == null)
        {
            return NotFound(new { message = "Resource not found" });
        }
        return Ok(result);
    }

    protected IActionResult HandleResult<T>(IEnumerable<T> result)
    {
        if (result == null || !result.Any())
        {
            return Ok(new List<T>());
        }
        return Ok(result);
    }

    protected IActionResult HandleError(string message, int statusCode = 400)
    {
        return StatusCode(statusCode, new { message, error = true });
    }

    protected IActionResult HandleSuccess(string message, object? data = null)
    {
        return Ok(new { message, data, success = true });
    }

    // Role-based access control helpers
    protected int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        return null;
    }

    protected string? GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value;
    }

    protected bool IsAdmin()
    {
        var role = GetCurrentUserRole();
        return role == "Admin" || role == "Owner";
    }

    protected bool IsOwner()
    {
        return GetCurrentUserRole() == "Owner";
    }

    protected bool IsSalesManager()
    {
        return GetCurrentUserRole() == "Sales Manager";
    }

    protected bool IsSalesPerson()
    {
        return GetCurrentUserRole() == "Sales Person";
    }

    protected bool IsClient()
    {
        return GetCurrentUserRole() == "Client";
    }

    protected bool IsHOD()
    {
        return GetCurrentUserRole() == "HOD";
    }

    protected bool IsCallingStaff()
    {
        return GetCurrentUserRole() == "Calling Staff";
    }

    protected bool IsEmployee()
    {
        return GetCurrentUserRole() == "Employee";
    }

    protected bool CanViewAll()
    {
        var role = GetCurrentUserRole();
        return IsAdmin() || role == "HOD" || role == "Calling Staff" || role == "Employee";
    }

    protected bool CanEdit()
    {
        return IsAdmin();
    }

    protected bool CanDelete()
    {
        return IsAdmin();
    }

    protected bool CanCreate()
    {
        return IsAdmin();
    }
}

