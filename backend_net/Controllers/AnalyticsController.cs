using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : BaseController
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
    }

    /// <summary>
    /// Get business analytics based on user role and accessible clients
    /// Available for: Admin, Owner, Sales Manager, Sales Person, Client
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAnalytics([FromQuery] GetAnalyticsRequest? request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            // Employee cannot view analytics
            if (IsEmployee())
            {
                return HandleError("Unauthorized: Employees cannot view analytics", 403);
            }

            // Client role cannot view analytics
            if (IsClient())
            {
                return HandleError("Unauthorized: Clients cannot view analytics", 403);
            }

            var analytics = await _analyticsService.GetAnalyticsAsync(userId.Value, request);
            return HandleSuccess("Analytics retrieved successfully", analytics);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }
}

