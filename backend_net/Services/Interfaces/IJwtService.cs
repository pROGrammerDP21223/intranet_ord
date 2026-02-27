using backend_net.Domain.Entities;

namespace backend_net.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
    int? ValidateToken(string token);
}

