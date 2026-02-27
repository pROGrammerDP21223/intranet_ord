namespace backend_net.Services.Interfaces;

public interface ICacheStatisticsService
{
    System.Threading.Tasks.Task<CacheStatistics> GetStatisticsAsync();
    System.Threading.Tasks.Task ClearStatisticsAsync();
}

public class CacheStatistics
{
    public long TotalRequests { get; set; }
    public long CacheHits { get; set; }
    public long CacheMisses { get; set; }
    public double HitRate { get; set; }
    public long TotalKeys { get; set; }
    public long MemoryUsage { get; set; }
    public string CacheType { get; set; } = string.Empty;
}

