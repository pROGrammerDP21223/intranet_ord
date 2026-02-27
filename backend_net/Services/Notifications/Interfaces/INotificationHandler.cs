using backend_net.Common.Results;
using backend_net.Domain.Entities;

namespace backend_net.Services.Notifications.Interfaces;

/// <summary>
/// Strategy pattern interface for notification handlers
/// Each notification type (Email, WhatsApp) implements this interface
/// </summary>
public interface INotificationHandler
{
    /// <summary>
    /// Notification type this handler supports
    /// </summary>
    string NotificationType { get; }

    /// <summary>
    /// Sends notification for an enquiry
    /// </summary>
    Task<Result> SendAsync(Enquiry enquiry, Client client, CancellationToken cancellationToken = default);
}

