using backend_net.Domain.Entities;

namespace backend_net.Services.Interfaces;

public interface ILogService
{
    System.Threading.Tasks.Task LogApiRequestAsync(
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
        string? queryString);
}

