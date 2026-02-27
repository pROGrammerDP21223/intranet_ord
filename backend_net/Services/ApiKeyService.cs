using backend_net.Data.Context;
using backend_net.Domain.Entities;
using backend_net.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace backend_net.Services;

public class ApiKeyService : IApiKeyService
{
    private readonly ApplicationDbContext _context;

    public ApiKeyService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiKey?> ValidateApiKeyAsync(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return null;

        var key = await _context.ApiKeys
            .Include(ak => ak.Client)
            .FirstOrDefaultAsync(ak => ak.Key == apiKey && !ak.IsDeleted);

        if (key == null)
            return null;

        // Check if key is active
        if (!key.IsActive)
            return null;

        // Check if key has expired
        if (key.ExpiresAt.HasValue && key.ExpiresAt.Value < DateTime.UtcNow)
            return null;

        return key;
    }

    public async Task<ApiKey> CreateApiKeyAsync(int clientId, string name, string? description = null, DateTime? expiresAt = null, string? allowedOrigins = null)
    {
        // Generate a secure API key
        var apiKeyValue = GenerateApiKey();

        var apiKey = new ApiKey
        {
            Key = apiKeyValue,
            Name = name,
            Description = description,
            ClientId = clientId,
            IsActive = true,
            ExpiresAt = expiresAt,
            AllowedOrigins = allowedOrigins,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.ApiKeys.AddAsync(apiKey);
        await _context.SaveChangesAsync();

        return apiKey;
    }

    public async Task<IEnumerable<ApiKey>> GetApiKeysByClientAsync(int clientId)
    {
        return await _context.ApiKeys
            .Include(ak => ak.Client)
            .Where(ak => ak.ClientId == clientId && !ak.IsDeleted)
            .OrderByDescending(ak => ak.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApiKey>> GetAllApiKeysAsync()
    {
        return await _context.ApiKeys
            .Include(ak => ak.Client)
            .Where(ak => !ak.IsDeleted)
            .OrderByDescending(ak => ak.CreatedAt)
            .ToListAsync();
    }

    public async Task<ApiKey?> GetApiKeyByIdAsync(int apiKeyId)
    {
        return await _context.ApiKeys
            .Include(ak => ak.Client)
            .FirstOrDefaultAsync(ak => ak.Id == apiKeyId && !ak.IsDeleted);
    }

    public async Task<ApiKey> UpdateApiKeyAsync(int apiKeyId, string? name = null, string? description = null, DateTime? expiresAt = null, string? allowedOrigins = null, bool? isActive = null)
    {
        var apiKey = await _context.ApiKeys.FindAsync(apiKeyId);
        if (apiKey == null || apiKey.IsDeleted)
        {
            throw new KeyNotFoundException("API key not found");
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            apiKey.Name = name;
        }

        if (description != null)
        {
            apiKey.Description = description;
        }

        if (expiresAt.HasValue)
        {
            apiKey.ExpiresAt = expiresAt;
        }

        if (allowedOrigins != null)
        {
            apiKey.AllowedOrigins = allowedOrigins;
        }

        if (isActive.HasValue)
        {
            apiKey.IsActive = isActive.Value;
        }

        apiKey.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return apiKey;
    }

    public async Task<bool> RevokeApiKeyAsync(int apiKeyId)
    {
        var apiKey = await _context.ApiKeys.FindAsync(apiKeyId);
        if (apiKey == null || apiKey.IsDeleted)
            return false;

        apiKey.IsActive = false;
        apiKey.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ReactivateApiKeyAsync(int apiKeyId)
    {
        var apiKey = await _context.ApiKeys.FindAsync(apiKeyId);
        if (apiKey == null || apiKey.IsDeleted)
            return false;

        // Check if key has expired
        if (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt.Value < DateTime.UtcNow)
        {
            return false; // Cannot reactivate expired key
        }

        apiKey.IsActive = true;
        apiKey.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> IsApiKeyValidAsync(string apiKey)
    {
        var key = await ValidateApiKeyAsync(apiKey);
        return key != null;
    }

    private string GenerateApiKey()
    {
        // Generate a secure random API key
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var keyBuilder = new StringBuilder();
        
        // Generate 64 character key
        for (int i = 0; i < 64; i++)
        {
            keyBuilder.Append(chars[random.Next(chars.Length)]);
        }

        return $"sk_" + keyBuilder.ToString();
    }
}

