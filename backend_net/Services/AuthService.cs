using System.Security.Cryptography;
using backend_net.Data.Context;
using backend_net.Data.Interfaces;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.DTOs.Responses;
using backend_net.Services.Interfaces;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly ApplicationDbContext _context;

    public AuthService(
        IUnitOfWork unitOfWork, 
        IJwtService jwtService, 
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
        _context = context;
    }

    private async Task<User?> LoadUserWithRoleAndPermissionsAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.Role)
                .ThenInclude(r => r!.RolePermissions!.Where(rp => !rp.IsDeleted))
                    .ThenInclude(rp => rp.Permission)
            .AsSplitQuery()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string? createdBy = null)
    {
        // Check if user already exists
        var existingUser = await _unitOfWork.Repository<User>().FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Get or assign default role (Employee)
        Role? role = null;
        if (request.RoleId.HasValue)
        {
            role = await _unitOfWork.Repository<Role>().GetByIdAsync(request.RoleId.Value);
            if (role == null)
            {
                throw new InvalidOperationException("Invalid role specified");
            }
        }
        else
        {
            // Default to Employee role (or first available role if Employee doesn't exist)
            role = await _unitOfWork.Repository<Role>().FirstOrDefaultAsync(r => r.Name == "Employee");
            if (role == null)
            {
                // Try to get any role as fallback
                var allRoles = await _unitOfWork.Repository<Role>().GetAllAsync();
                role = allRoles.FirstOrDefault();
                if (role == null)
                {
                    throw new InvalidOperationException("No roles found in database. Please ensure roles are seeded.");
                }
            }
        }

        // Create new user
        var user = new User
        {
            Name = request.Name,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = true,
            RoleId = role.Id,
            Role = role,
            CreatedBy = createdBy,
            UpdatedBy = createdBy,
            LastUpdatedBy = createdBy
        };

        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Reload user with role and permissions for token generation
        user = await LoadUserWithRoleAndPermissionsAsync(user.Id);

        // Generate tokens
        var token = _jwtService.GenerateToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                RoleId = user.RoleId,
                RoleName = user.Role?.Name,
                Permissions = user.Role?.RolePermissions != null 
                    ? string.Join(",", user.Role.RolePermissions
                        .Where(rp => rp.Permission != null)
                        .Select(rp => rp.Permission!.Name))
                    : null
            }
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Load user with role and permissions in one query
        // Use AsSplitQuery to avoid Cartesian explosion and ignore soft delete filter for RolePermissions
        var user = await _context.Users
            .Include(u => u.Role)
                .ThenInclude(r => r!.RolePermissions!.Where(rp => !rp.IsDeleted))
                    .ThenInclude(rp => rp.Permission)
            .AsSplitQuery()
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant() && !u.IsDeleted);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is inactive");
        }

        // Update last updated by
        user.LastUpdatedBy = user.Email;
        await _unitOfWork.Repository<User>().UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Generate tokens
        var token = _jwtService.GenerateToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                RoleId = user.RoleId,
                RoleName = user.Role?.Name,
                Permissions = user.Role?.RolePermissions != null 
                    ? string.Join(",", user.Role.RolePermissions
                        .Where(rp => rp.Permission != null)
                        .Select(rp => rp.Permission!.Name))
                    : null
            }
        };
    }

    public async Task<bool> ValidateUserAsync(string email, string password)
    {
        var user = await _unitOfWork.Repository<User>().FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());

        if (user == null || !user.IsActive)
        {
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public async Task<UserResponse?> GetUserByIdAsync(int userId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);

        if (user == null)
        {
            return null;
        }

        // Load user with role and permissions
        user = await LoadUserWithRoleAndPermissionsAsync(userId);
        if (user == null)
        {
            return null;
        }

        return new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name
        };
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await _unitOfWork.Repository<User>().FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());
        
        // Always return true to prevent email enumeration attacks
        if (user != null && user.IsActive)
        {
            // Generate secure reset token
            var resetToken = GenerateSecureToken();
            var expiresAt = DateTime.UtcNow.AddHours(1); // Token expires in 1 hour

            // Invalidate any existing reset tokens for this user
            var existingTokens = await _unitOfWork.Repository<PasswordResetToken>()
                .FindAsync(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);
            
            foreach (var token in existingTokens)
            {
                token.IsUsed = true;
                token.UsedAt = DateTime.UtcNow;
                await _unitOfWork.Repository<PasswordResetToken>().UpdateAsync(token);
            }

            // Create new reset token
            var passwordResetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = resetToken,
                ExpiresAt = expiresAt,
                IsUsed = false
            };

            await _unitOfWork.Repository<PasswordResetToken>().AddAsync(passwordResetToken);
            await _unitOfWork.SaveChangesAsync();

            // Get frontend URL from configuration
            var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var resetUrl = $"{frontendUrl}/reset-password";

            // Send email
            var emailSent = await _emailService.SendPasswordResetEmailAsync(
                user.Email, 
                user.Name, 
                resetToken, 
                resetUrl);

            if (!emailSent)
            {
                _logger.LogWarning($"Failed to send password reset email to {email}");
            }
        }
        
        return true; // Always return true to prevent email enumeration
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        // Find valid reset token
        var resetToken = await _unitOfWork.Repository<PasswordResetToken>()
            .FirstOrDefaultAsync(t => 
                t.Token == request.Token && 
                !t.IsUsed && 
                t.ExpiresAt > DateTime.UtcNow);

        if (resetToken == null)
        {
            throw new InvalidOperationException("Invalid or expired reset token");
        }

        // Find user
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(resetToken.UserId);
        if (user == null || user.Email.ToLowerInvariant() != request.Email.ToLowerInvariant())
        {
            throw new InvalidOperationException("Invalid email or token");
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("User account is inactive");
        }

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<User>().UpdateAsync(user);

        // Mark token as used
        resetToken.IsUsed = true;
        resetToken.UsedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<PasswordResetToken>().UpdateAsync(resetToken);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}

