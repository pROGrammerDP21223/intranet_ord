using backend_net.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend_net.Services;

public class CacheStatisticsService : ICacheStatisticsService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheStatisticsService> _logger;
    private static long _totalRequests = 0;
    private static long _cacheHits = 0;
    private static long _cacheMisses = 0;
    private static readonly object _lock = new object();

    public CacheStatisticsService(ICacheService cacheService, ILogger<CacheStatisticsService> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public System.Threading.Tasks.Task<CacheStatistics> GetStatisticsAsync()
    {
        lock (_lock)
        {
            var hitRate = _totalRequests > 0 
                ? (double)_cacheHits / _totalRequests * 100 
                : 0;

            return System.Threading.Tasks.Task.FromResult(new CacheStatistics
            {
                TotalRequests = _totalRequests,
                CacheHits = _cacheHits,
                CacheMisses = _cacheMisses,
                HitRate = Math.Round(hitRate, 2),
                TotalKeys = 0, // Would need cache implementation details
                MemoryUsage = GC.GetTotalMemory(false),
                CacheType = _cacheService.GetType().Name
            });
        }
    }

    public async System.Threading.Tasks.Task ClearStatisticsAsync()
    {
        lock (_lock)
        {
            _totalRequests = 0;
            _cacheHits = 0;
            _cacheMisses = 0;
        }
        await System.Threading.Tasks.Task.CompletedTask;
    }

    public static void RecordCacheHit()
    {
        lock (_lock)
        {
            _totalRequests++;
            _cacheHits++;
        }
    }

    public static void RecordCacheMiss()
    {
        lock (_lock)
        {
            _totalRequests++;
            _cacheMisses++;
        }
    }
}

