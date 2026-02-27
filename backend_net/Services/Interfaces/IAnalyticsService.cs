using backend_net.DTOs.Requests;
using backend_net.DTOs.Responses;

namespace backend_net.Services.Interfaces;

public interface IAnalyticsService
{
    Task<AnalyticsResponse> GetAnalyticsAsync(int userId, GetAnalyticsRequest? request = null, CancellationToken cancellationToken = default);
}

