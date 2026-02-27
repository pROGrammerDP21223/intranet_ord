using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace backend_net;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var user = httpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            return false;
        }

        // Only Admin and Owner can access Hangfire dashboard
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        return role == "Admin" || role == "Owner";
    }
}

