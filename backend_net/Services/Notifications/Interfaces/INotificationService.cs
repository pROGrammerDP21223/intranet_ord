using backend_net.Common.Results;
using backend_net.Domain.Entities;

namespace backend_net.Services.Notifications.Interfaces;

/// <summary>
/// Notification service interface
/// Orchestrates sending notifications through various channels
/// </summary>
public interface INotificationService
{
    Task<Result> SendEnquiryNotificationsAsync(Enquiry enquiry, Client client, CancellationToken cancellationToken = default);
}

