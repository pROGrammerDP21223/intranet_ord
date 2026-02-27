using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CacheStatisticsController : BaseController
{
    private readonly ICacheStatisticsService _cacheStatisticsService;

    public CacheStatisticsController(ICacheStatisticsService cacheStatisticsService)
    {
        _cacheStatisticsService = cacheStatisticsService ?? throw new ArgumentNullException(nameof(cacheStatisticsService));
    }

    /// <summary>
    /// Get cache statistics
    /// Only Admin and Owner can view cache statistics
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view cache statistics", 403);
            }

            var statistics = await _cacheStatisticsService.GetStatisticsAsync();
            return HandleSuccess("Cache statistics retrieved successfully", statistics);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Clear cache statistics
    /// Only Admin and Owner can clear cache statistics
    /// </summary>
    [HttpPost("clear")]
    public async Task<IActionResult> ClearStatistics()
    {
        try
        {
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can clear cache statistics", 403);
            }

            await _cacheStatisticsService.ClearStatisticsAsync();
            return HandleSuccess("Cache statistics cleared successfully", null);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }
}

