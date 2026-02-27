using backend_net.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace backend_net.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;
    private readonly Dictionary<string, DateTime> _keyExpiration = new();

    public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            if (_memoryCache.TryGetValue(key, out var cachedValue))
            {
                if (cachedValue is T typedValue)
                {
                    return Task.FromResult<T?>(typedValue);
                }

                // Try to deserialize if it's a string
                if (cachedValue is string jsonString)
                {
                    var deserialized = JsonSerializer.Deserialize<T>(jsonString);
                    return Task.FromResult(deserialized);
                }
            }

            return Task.FromResult<T?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache key {Key}", key);
            return Task.FromResult<T?>(null);
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var options = new MemoryCacheEntryOptions();

            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration.Value;
            }
            else
            {
                // Default expiration: 1 hour
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            }

            options.SlidingExpiration = TimeSpan.FromMinutes(15);

            _memoryCache.Set(key, value, options);
            _keyExpiration[key] = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromHours(1));

            _logger.LogDebug("Cache set for key {Key} with expiration {Expiration}", key, expiration);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache key {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            _memoryCache.Remove(key);
            _keyExpiration.Remove(key);
            _logger.LogDebug("Cache removed for key {Key}", key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache key {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            // Note: IMemoryCache doesn't support pattern-based removal natively
            // This is a simplified implementation that removes keys containing the pattern
            var keysToRemove = _keyExpiration.Keys.Where(k => k.Contains(pattern, StringComparison.OrdinalIgnoreCase)).ToList();
            
            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _keyExpiration.Remove(key);
            }

            _logger.LogDebug("Cache removed for pattern {Pattern}, {Count} keys removed", pattern, keysToRemove.Count);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache by pattern {Pattern}", pattern);
            return Task.CompletedTask;
        }
    }

    public Task ClearAsync()
    {
        try
        {
            // IMemoryCache doesn't have a Clear method, so we need to track keys
            var keysToRemove = _keyExpiration.Keys.ToList();
            
            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
            }

            _keyExpiration.Clear();
            _logger.LogDebug("Cache cleared, {Count} keys removed", keysToRemove.Count);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            return Task.CompletedTask;
        }
    }

    public Task<bool> ExistsAsync(string key)
    {
        try
        {
            var exists = _memoryCache.TryGetValue(key, out _);
            return Task.FromResult(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache key existence {Key}", key);
            return Task.FromResult(false);
        }
    }
}

