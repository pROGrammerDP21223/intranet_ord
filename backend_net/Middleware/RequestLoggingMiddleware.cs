using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using backend_net.Services.Interfaces;

namespace backend_net.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly IServiceProvider _serviceProvider;

    public RequestLoggingMiddleware(
        RequestDelegate next, 
        ILogger<RequestLoggingMiddleware> logger,
        IServiceProvider serviceProvider)
    {
        _next = next;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;
        var queryString = context.Request.QueryString.ToString();

        // Enable request body buffering so we can read it
        context.Request.EnableBuffering();

        string? requestBody = null;
        if (context.Request.ContentLength > 0 && 
            (requestMethod == "POST" || requestMethod == "PUT" || requestMethod == "PATCH"))
        {
            try
            {
                using var reader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true);
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0; // Reset stream position
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read request body");
            }
        }

        // Enable response body buffering
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        _logger.LogInformation(
            "Incoming {Method} request to {Path}",
            requestMethod,
            requestPath);

        try
        {
            await _next(context);
            stopwatch.Stop();

            // Read response body
            string? responseBodyContent = null;
            responseBody.Seek(0, SeekOrigin.Begin);
            if (responseBody.Length > 0)
            {
                responseBodyContent = await new StreamReader(responseBody).ReadToEndAsync();
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }

            var statusCode = context.Response.StatusCode;

            _logger.LogInformation(
                "Completed {Method} request to {Path} in {ElapsedMilliseconds}ms with status {StatusCode}",
                requestMethod,
                requestPath,
                stopwatch.ElapsedMilliseconds,
                statusCode);

            // Only log successful API requests (2xx status codes)
            if (statusCode >= 200 && statusCode < 300)
            {
                // Extract user information from claims
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                var userNameClaim = context.User.FindFirst(ClaimTypes.Name);
                var userEmailClaim = context.User.FindFirst(ClaimTypes.Email);
                var userRoleClaim = context.User.FindFirst(ClaimTypes.Role);

                int? userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
                string? userName = userNameClaim?.Value;
                string? userEmail = userEmailClaim?.Value;
                string? userRole = userRoleClaim?.Value;

                // Extract controller and action from route
                var routeData = context.Request.RouteValues;
                string? controller = routeData.ContainsKey("controller") 
                    ? routeData["controller"]?.ToString() 
                    : null;
                string? action = routeData.ContainsKey("action") 
                    ? routeData["action"]?.ToString() 
                    : null;

                // Get IP address
                var ipAddress = context.Connection.RemoteIpAddress?.ToString();
                if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
                {
                    ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                }

                // Get user agent
                var userAgent = context.Request.Headers["User-Agent"].ToString();

                // Log to database asynchronously (fire and forget)
                // Create a service scope to resolve scoped services
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var logService = scope.ServiceProvider.GetRequiredService<ILogService>();
                        
                        await logService.LogApiRequestAsync(
                            httpMethod: requestMethod,
                            endpoint: requestPath.ToString(),
                            controller: controller,
                            action: action,
                            userId: userId,
                            userName: userName,
                            userEmail: userEmail,
                            userRole: userRole,
                            statusCode: statusCode,
                            ipAddress: ipAddress,
                            userAgent: userAgent,
                            requestBody: requestBody,
                            responseBody: responseBodyContent,
                            responseTimeMs: stopwatch.ElapsedMilliseconds,
                            queryString: queryString);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to log API request to database");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Failed {Method} request to {Path} in {ElapsedMilliseconds}ms",
                requestMethod,
                requestPath,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}

