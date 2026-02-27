using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using backend_net.Services.Interfaces;

namespace backend_net.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireApiKeyAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var apiKeyService = context.HttpContext.RequestServices.GetRequiredService<IApiKeyService>();
        
        // Get API key from header
        if (!context.HttpContext.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
        {
            context.Result = new UnauthorizedObjectResult(new { message = "API Key is required. Please provide X-API-Key header.", error = true });
            return;
        }

        var apiKey = apiKeyHeader.ToString();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            context.Result = new UnauthorizedObjectResult(new { message = "API Key is invalid.", error = true });
            return;
        }

        // Validate API key
        var validApiKey = await apiKeyService.ValidateApiKeyAsync(apiKey);
        if (validApiKey == null)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Invalid or expired API Key.", error = true });
            return;
        }

        // Store client ID in HttpContext for use in controller
        context.HttpContext.Items["ClientId"] = validApiKey.ClientId;
        context.HttpContext.Items["ApiKey"] = validApiKey;
    }
}

