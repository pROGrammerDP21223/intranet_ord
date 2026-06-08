using backend_net.Data.Context;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend_net.Middleware;

/// <summary>
/// Extends the default CORS policy to dynamically allow origins stored on API keys.
/// When the request's Origin header matches the AllowedOrigins field of any active API key,
/// a permissive CORS policy is returned for that origin. Otherwise, falls back to the
/// statically configured "AllowFrontend" policy.
/// </summary>
public class DynamicApiKeyCorsPolicyProvider : ICorsPolicyProvider
{
    private readonly DefaultCorsPolicyProvider _default;
    private readonly IServiceProvider _serviceProvider;

    public DynamicApiKeyCorsPolicyProvider(IOptions<CorsOptions> corsOptions, IServiceProvider serviceProvider)
    {
        _default = new DefaultCorsPolicyProvider(corsOptions);
        _serviceProvider = serviceProvider;
    }

    public async Task<CorsPolicy?> GetPolicyAsync(HttpContext context, string? policyName)
    {
        var origin = context.Request.Headers.Origin.ToString();

        if (!string.IsNullOrEmpty(origin))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Load AllowedOrigins for all active, non-expired API keys
                var allowedOriginsList = await dbContext.ApiKeys
                    .Where(ak => ak.IsActive && !ak.IsDeleted &&
                                 (ak.ExpiresAt == null || ak.ExpiresAt > DateTime.UtcNow) &&
                                 ak.AllowedOrigins != null && ak.AllowedOrigins != "")
                    .Select(ak => ak.AllowedOrigins!)
                    .ToListAsync();

                // Exact, case-insensitive match against comma-separated origin list
                var isAllowed = allowedOriginsList.Any(origins =>
                    origins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                           .Contains(origin, StringComparer.OrdinalIgnoreCase));

                if (isAllowed)
                {
                    return new CorsPolicyBuilder()
                        .WithOrigins(origin)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .WithExposedHeaders("Content-Disposition", "Content-Length", "Content-Type", "Authorization")
                        .Build();
                }
            }
            catch
            {
                // DB unavailable — fall through to the default static policy
            }
        }

        return await _default.GetPolicyAsync(context, policyName);
    }
}
