using backend_net.Data.Context;
using backend_net.Domain.Entities;
using backend_net.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Services;

public class LogService : ILogService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LogService> _logger;

    public LogService(ApplicationDbContext context, ILogger<LogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task LogApiRequestAsync(
        string httpMethod,
        string endpoint,
        string? controller,
        string? action,
        int? userId,
        string? userName,
        string? userEmail,
        string? userRole,
        int statusCode,
        string? ipAddress,
        string? userAgent,
        string? requestBody,
        string? responseBody,
        long? responseTimeMs,
        string? queryString)
    {
        try
        {
            // Truncate long strings to fit database constraints
            var log = new Log
            {
                HttpMethod = TruncateString(httpMethod, 10),
                Endpoint = TruncateString(endpoint, 500),
                Controller = TruncateString(controller, 50),
                Action = TruncateString(action, 50),
                UserId = userId,
                UserName = TruncateString(userName, 255),
                UserEmail = TruncateString(userEmail, 255),
                UserRole = TruncateString(userRole, 50),
                StatusCode = statusCode,
                IpAddress = TruncateString(ipAddress, 45),
                UserAgent = TruncateString(userAgent, 500),
                RequestBody = TruncateString(requestBody, 10000) ?? null, // TEXT field, but we'll limit to 10KB
                ResponseBody = TruncateString(responseBody, 10000) ?? null, // TEXT field, but we'll limit to 10KB
                ResponseTimeMs = responseTimeMs,
                QueryString = TruncateString(queryString, 500),
                CreatedAt = DateTime.UtcNow
            };

            await _context.Logs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log to Serilog but don't throw - we don't want logging failures to break the app
            _logger.LogError(ex, "Failed to save API request log to database");
        }
    }

    private static string? TruncateString(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Length > maxLength ? value.Substring(0, maxLength) : value;
    }
}

