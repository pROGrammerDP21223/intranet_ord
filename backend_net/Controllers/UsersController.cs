using backend_net.Controllers;
using backend_net.Data.Context;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[Authorize]
public class UsersController : BaseController
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            // Only Admin/Owner can view all users
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view users", 403);
            }
            var users = await _context.Users
                .Include(u => u.Role)
                .Where(u => !u.IsDeleted)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.IsActive,
                    u.RoleId,
                    RoleName = u.Role != null ? u.Role.Name : null,
                    u.CreatedAt,
                    u.UpdatedAt
                })
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return HandleSuccess("Users retrieved successfully", users);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        try
        {
            // Only Admin/Owner can view users
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view users", 403);
            }
            var user = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Id == id && !u.IsDeleted)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.IsActive,
                    u.RoleId,
                    RoleName = u.Role != null ? u.Role.Name : null,
                    u.CreatedAt,
                    u.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return HandleError("User not found", 404);
            }

            return HandleSuccess("User retrieved successfully", user);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            // Only Admin/Owner can create users
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can create users", 403);
            }
            if (!ModelState.IsValid)
            {
                return HandleError("Invalid request data", 400);
            }

            // Check if user with same email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant() && !u.IsDeleted);

            if (existingUser != null)
            {
                return HandleError("User with this email already exists", 400);
            }

            // Validate role exists
            var role = await _context.Roles.FindAsync(request.RoleId);
            if (role == null || role.IsDeleted)
            {
                return HandleError("Invalid role specified", 400);
            }

            var user = new User
            {
                Name = request.Name,
                Email = request.Email.ToLowerInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsActive = request.IsActive,
                RoleId = request.RoleId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Reload with role
            var createdUser = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Id == user.Id)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.IsActive,
                    u.RoleId,
                    RoleName = u.Role != null ? u.Role.Name : null,
                    u.CreatedAt
                })
                .FirstOrDefaultAsync();

            return HandleSuccess("User created successfully", createdUser);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            // Only Admin/Owner can update users
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can update users", 403);
            }
            if (!ModelState.IsValid)
            {
                return HandleError("Invalid request data", 400);
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null || user.IsDeleted)
            {
                return HandleError("User not found", 404);
            }

            // Check if email is being changed and if it's already taken
            if (user.Email != request.Email.ToLowerInvariant())
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant() && !u.IsDeleted && u.Id != id);

                if (existingUser != null)
                {
                    return HandleError("Email already in use by another user", 400);
                }
            }

            // Validate role exists
            var role = await _context.Roles.FindAsync(request.RoleId);
            if (role == null || role.IsDeleted)
            {
                return HandleError("Invalid role specified", 400);
            }

            user.Name = request.Name;
            user.Email = request.Email.ToLowerInvariant();
            user.IsActive = request.IsActive;
            user.RoleId = request.RoleId;
            user.UpdatedAt = DateTime.UtcNow;

            // Update password only if provided
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            await _context.SaveChangesAsync();

            // Reload with role
            var updatedUser = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.IsActive,
                    u.RoleId,
                    RoleName = u.Role != null ? u.Role.Name : null,
                    u.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return HandleSuccess("User updated successfully", updatedUser);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            // Only Admin/Owner can delete users
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can delete users", 403);
            }
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.IsDeleted)
            {
                return HandleError("User not found", 404);
            }

            // Soft delete
            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return HandleSuccess("User deleted successfully");
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }
}

public class CreateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
    public int RoleId { get; set; }
    public bool IsActive { get; set; }
}
