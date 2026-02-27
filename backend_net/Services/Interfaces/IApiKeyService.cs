using backend_net.Domain.Entities;

namespace backend_net.Services.Interfaces;

public interface IApiKeyService
{
    Task<ApiKey?> ValidateApiKeyAsync(string apiKey);
    Task<ApiKey> CreateApiKeyAsync(int clientId, string name, string? description = null, DateTime? expiresAt = null, string? allowedOrigins = null);
    Task<IEnumerable<ApiKey>> GetApiKeysByClientAsync(int clientId);
    Task<IEnumerable<ApiKey>> GetAllApiKeysAsync();
    Task<ApiKey?> GetApiKeyByIdAsync(int apiKeyId);
    Task<ApiKey> UpdateApiKeyAsync(int apiKeyId, string? name = null, string? description = null, DateTime? expiresAt = null, string? allowedOrigins = null, bool? isActive = null);
    Task<bool> RevokeApiKeyAsync(int apiKeyId);
    Task<bool> ReactivateApiKeyAsync(int apiKeyId);
    Task<bool> IsApiKeyValidAsync(string apiKey);
}

