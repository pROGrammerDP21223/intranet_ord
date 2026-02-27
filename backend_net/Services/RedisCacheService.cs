using backend_net.Services.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace backend_net.Services;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly bool _isRedisAvailable;

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        if (redis == null)
        {
            _logger.LogWarning("Redis connection not available. Redis caching will be disabled.");
            _isRedisAvailable = false;
            _redis = null!;
            _database = null!;
            return;
        }

        try
        {
            _redis = redis;
            _database = _redis.GetDatabase();
            _isRedisAvailable = _redis.IsConnected;
            if (_isRedisAvailable)
            {
                _logger.LogInformation("Redis connection established successfully");
            }
            else
            {
                _logger.LogWarning("Redis connection is not connected. Redis caching will be disabled.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Redis. Redis caching will be disabled.");
            _isRedisAvailable = false;
            _redis = null!;
            _database = null!;
        }
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        if (!_isRedisAvailable)
        {
            return null;
        }

        try
        {
            var value = await _database.StringGetAsync(key);
            if (!value.HasValue)
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache key {Key} from Redis", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        if (!_isRedisAvailable)
        {
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(value);
            if (expiration.HasValue)
            {
                await _database.StringSetAsync(key, json, expiration.Value);
            }
            else
            {
                // Default expiration: 1 hour
                await _database.StringSetAsync(key, json, TimeSpan.FromHours(1));
            }

            _logger.LogDebug("Cache set for key {Key} in Redis with expiration {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache key {Key} in Redis", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        if (!_isRedisAvailable)
        {
            return;
        }

        try
        {
            await _database.KeyDeleteAsync(key);
            _logger.LogDebug("Cache removed for key {Key} in Redis", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache key {Key} from Redis", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        if (!_isRedisAvailable)
        {
            return;
        }

        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern);
            
            foreach (var key in keys)
            {
                await _database.KeyDeleteAsync(key);
            }

            _logger.LogDebug("Cache removed for pattern {Pattern} in Redis, {Count} keys removed", pattern, keys.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache by pattern {Pattern} from Redis", pattern);
        }
    }

    public async Task ClearAsync()
    {
        if (!_isRedisAvailable)
        {
            return;
        }

        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            await server.FlushDatabaseAsync();
            _logger.LogDebug("Redis cache cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing Redis cache");
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        if (!_isRedisAvailable)
        {
            return false;
        }

        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache key existence {Key} in Redis", key);
            return false;
        }
    }
}

