using backend_net.DTOs.Requests;
using backend_net.DTOs.Responses;

namespace backend_net.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string? createdBy = null);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<bool> ValidateUserAsync(string email, string password);
    Task<UserResponse?> GetUserByIdAsync(int userId);
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
}

