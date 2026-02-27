using backend_net.Common.Results;
using backend_net.Domain.Entities;
using backend_net.Services.Notifications.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend_net.Services.Notifications;

/// <summary>
/// Notification service using Strategy pattern
/// Orchestrates multiple notification handlers
/// Follows Open/Closed Principle - can add new notification types without modifying this class
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IEnumerable<INotificationHandler> _handlers;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IEnumerable<INotificationHandler> handlers,
        ILogger<NotificationService> logger)
    {
        _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> SendEnquiryNotificationsAsync(Enquiry enquiry, Client client, CancellationToken cancellationToken = default)
    {
        if (enquiry == null)
            return Result.Failure("Enquiry cannot be null");

        if (client == null)
            return Result.Failure("Client cannot be null");

        var handlersList = _handlers.ToList();
        if (!handlersList.Any())
        {
            _logger.LogWarning("No notification handlers registered");
            return Result.Failure("No notification handlers available");
        }

        var results = new List<Result>();

        // Send notifications using all registered handlers
        foreach (var handler in handlersList)
        {
            try
            {
                var result = await handler.SendAsync(enquiry, client, cancellationToken);
                results.Add(result);

                if (result.IsFailure)
                {
                    _logger.LogWarning(
                        "Notification handler {HandlerType} failed for enquiry {EnquiryId}: {Error}",
                        handler.NotificationType,
                        enquiry.Id,
                        result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Exception in notification handler {HandlerType} for enquiry {EnquiryId}",
                    handler.NotificationType,
                    enquiry.Id);
                results.Add(Result.Failure($"Exception in {handler.NotificationType} handler: {ex.Message}"));
            }
        }

        // Consider it successful if at least one notification succeeded
        // Email is critical, WhatsApp is optional
        var hasSuccess = results.Any(r => r.IsSuccess);

        if (hasSuccess)
        {
            _logger.LogInformation("Notifications sent for enquiry {EnquiryId}. Success: {SuccessCount}, Failed: {FailedCount}",
                enquiry.Id,
                results.Count(r => r.IsSuccess),
                results.Count(r => r.IsFailure));
        }
        else if (results.Any())
        {
            _logger.LogWarning("All notification handlers failed for enquiry {EnquiryId}", enquiry.Id);
        }

        return hasSuccess ? Result.Success() : Result.Failure("All notification handlers failed");
    }
}

