using backend_net.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace backend_net.Services;

/// <summary>
/// Service for sending real-time notifications via SignalR
/// </summary>
public interface ISignalRNotificationService
{
    System.Threading.Tasks.Task NotifyNewEnquiryAsync(Enquiry enquiry, Client client);
    System.Threading.Tasks.Task NotifyTicketUpdateAsync(Ticket ticket, string action);
    System.Threading.Tasks.Task NotifyClientApprovalAsync(Client client, int approvedByUserId);
    System.Threading.Tasks.Task NotifyTransactionUpdateAsync(Transaction transaction, string action);
    System.Threading.Tasks.Task NotifyUserAsync(int userId, string title, string message, string? type = "info");
    System.Threading.Tasks.Task NotifyGroupAsync(string groupName, string title, string message, string? type = "info");
    System.Threading.Tasks.Task NotifyInternalMessageAsync(Message message);
    System.Threading.Tasks.Task NotifyEventUpdateAsync(Event @event, string action);
}

public class SignalRNotificationService : ISignalRNotificationService
{
    private readonly IHubContext<Hubs.NotificationHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
        IHubContext<Hubs.NotificationHub> hubContext,
        ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task NotifyNewEnquiryAsync(Enquiry enquiry, Client client)
    {
        try
        {
            var notification = new
            {
                Type = "enquiry",
                Title = "New Enquiry Received",
                Message = $"New enquiry from {enquiry.FullName} for client {client.CompanyName}",
                Data = new
                {
                    EnquiryId = enquiry.Id,
                    ClientId = client.Id,
                    ClientName = client.CompanyName,
                    EnquirerName = enquiry.FullName,
                    EnquirerEmail = enquiry.EmailId,
                    EnquirerPhone = enquiry.MobileNumber,
                    Status = enquiry.Status,
                    CreatedAt = enquiry.CreatedAt
                },
                Timestamp = DateTime.UtcNow
            };

            // Notify all admins and owners
            await _hubContext.Clients.Group("role_admin").SendAsync("ReceiveNotification", notification);
            await _hubContext.Clients.Group("role_owner").SendAsync("ReceiveNotification", notification);
            await _hubContext.Clients.Group("role_calling_staff").SendAsync("ReceiveNotification", notification);
            await _hubContext.Clients.Group("role_hod").SendAsync("ReceiveNotification", notification);

            // Notify client if they have a user account
            if (client.CreatedByUserId.HasValue)
            {
                await _hubContext.Clients.Group($"user_{client.CreatedByUserId.Value}").SendAsync("ReceiveNotification", notification);
            }

            _logger.LogInformation("SignalR notification sent for new enquiry {EnquiryId}", enquiry.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification for enquiry {EnquiryId}", enquiry.Id);
        }
    }

    public async System.Threading.Tasks.Task NotifyTicketUpdateAsync(Ticket ticket, string action)
    {
        try
        {
            var notification = new
            {
                Type = "ticket",
                Title = $"Ticket {action}",
                Message = $"Ticket #{ticket.TicketNumber} has been {action}",
                Data = new
                {
                    TicketId = ticket.Id,
                    TicketNumber = ticket.TicketNumber,
                    Title = ticket.Title,
                    Status = ticket.Status,
                    Priority = ticket.Priority,
                    ClientId = ticket.ClientId,
                    AssignedTo = ticket.AssignedTo,
                    Action = action,
                    UpdatedAt = ticket.UpdatedAt ?? ticket.CreatedAt
                },
                Timestamp = DateTime.UtcNow
            };

            // Notify ticket creator
            if (ticket.CreatedBy > 0)
            {
                await _hubContext.Clients.Group($"user_{ticket.CreatedBy}").SendAsync("ReceiveNotification", notification);
            }

            // Notify assigned user
            if (ticket.AssignedTo.HasValue)
            {
                await _hubContext.Clients.Group($"user_{ticket.AssignedTo.Value}").SendAsync("ReceiveNotification", notification);
            }

            // Notify admins and staff
            await _hubContext.Clients.Group("role_admin").SendAsync("ReceiveNotification", notification);
            await _hubContext.Clients.Group("role_owner").SendAsync("ReceiveNotification", notification);
            await _hubContext.Clients.Group("role_calling_staff").SendAsync("ReceiveNotification", notification);

            _logger.LogInformation("SignalR notification sent for ticket {TicketId} action {Action}", ticket.Id, action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification for ticket {TicketId}", ticket.Id);
        }
    }

    public async System.Threading.Tasks.Task NotifyClientApprovalAsync(Client client, int approvedByUserId)
    {
        try
        {
            var notification = new
            {
                Type = "client",
                Title = "Client Approved",
                Message = $"Client {client.CompanyName} has been approved",
                Data = new
                {
                    ClientId = client.Id,
                    ClientName = client.CompanyName,
                    CustomerNo = client.CustomerNo,
                    ApprovedBy = approvedByUserId,
                    ApprovedAt = DateTime.UtcNow
                },
                Timestamp = DateTime.UtcNow
            };

            // Notify client if they have a user account
            if (client.CreatedByUserId.HasValue)
            {
                await _hubContext.Clients.Group($"user_{client.CreatedByUserId.Value}").SendAsync("ReceiveNotification", notification);
            }

            // Notify sales person if assigned
            // This would require querying the sales person assignments

            _logger.LogInformation("SignalR notification sent for client approval {ClientId}", client.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification for client {ClientId}", client.Id);
        }
    }

    public async System.Threading.Tasks.Task NotifyTransactionUpdateAsync(Transaction transaction, string action)
    {
        try
        {
            var notification = new
            {
                Type = "transaction",
                Title = $"Transaction {action}",
                Message = $"Transaction {transaction.TransactionNumber} has been {action}",
                Data = new
                {
                    TransactionId = transaction.Id,
                    TransactionNumber = transaction.TransactionNumber,
                    ClientId = transaction.ClientId,
                    Amount = transaction.Amount,
                    Type = transaction.TransactionType,
                    Action = action,
                    UpdatedAt = transaction.UpdatedAt ?? transaction.CreatedAt
                },
                Timestamp = DateTime.UtcNow
            };

            // Notify admins, owners, and sales managers
            await _hubContext.Clients.Group("role_admin").SendAsync("ReceiveNotification", notification);
            await _hubContext.Clients.Group("role_owner").SendAsync("ReceiveNotification", notification);
            await _hubContext.Clients.Group("role_sales_manager").SendAsync("ReceiveNotification", notification);

            _logger.LogInformation("SignalR notification sent for transaction {TransactionId} action {Action}", transaction.Id, action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification for transaction {TransactionId}", transaction.Id);
        }
    }

    public async System.Threading.Tasks.Task NotifyUserAsync(int userId, string title, string message, string? type = "info")
    {
        try
        {
            var notification = new
            {
                Type = type,
                Title = title,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notification);
            _logger.LogInformation("SignalR notification sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification to user {UserId}", userId);
        }
    }

    public async System.Threading.Tasks.Task NotifyGroupAsync(string groupName, string title, string message, string? type = "info")
    {
        try
        {
            var notification = new
            {
                Type = type,
                Title = title,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
            _logger.LogInformation("SignalR notification sent to group {GroupName}", groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification to group {GroupName}", groupName);
        }
    }

    public async System.Threading.Tasks.Task NotifyInternalMessageAsync(Message message)
    {
        try
        {
            var notification = new
            {
                Type = "message",
                Title = "New Message",
                Message = $"You have a new message from {message.Sender?.Name ?? "Unknown"}",
                Data = new
                {
                    MessageId = message.Id,
                    SenderId = message.SenderId,
                    SenderName = message.Sender?.Name,
                    Subject = message.Subject,
                    Content = message.Content,
                    CreatedAt = message.CreatedAt
                },
                Timestamp = DateTime.UtcNow
            };

            // Notify recipient
            await _hubContext.Clients.Group($"user_{message.RecipientId}").SendAsync("ReceiveNotification", notification);
            _logger.LogInformation("SignalR notification sent for message {MessageId} to user {RecipientId}", message.Id, message.RecipientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification for message {MessageId}", message.Id);
        }
    }

    public async System.Threading.Tasks.Task NotifyEventUpdateAsync(Event @event, string action)
    {
        try
        {
            var notification = new
            {
                Type = "event",
                Title = $"Event {action}",
                Message = $"Event '{@event.Title}' has been {action}",
                Data = new
                {
                    EventId = @event.Id,
                    Title = @event.Title,
                    StartDate = @event.StartDate,
                    EndDate = @event.EndDate,
                    Location = @event.Location,
                    Status = @event.Status,
                    Action = action,
                    CreatedAt = @event.CreatedAt
                },
                Timestamp = DateTime.UtcNow
            };

            // Notify event owner
            if (@event.UserId.HasValue)
            {
                await _hubContext.Clients.Group($"user_{@event.UserId.Value}").SendAsync("ReceiveNotification", notification);
            }

            // Notify admins and owners
            await _hubContext.Clients.Group("role_admin").SendAsync("ReceiveNotification", notification);
            await _hubContext.Clients.Group("role_owner").SendAsync("ReceiveNotification", notification);

            _logger.LogInformation("SignalR notification sent for event {EventId} action {Action}", @event.Id, action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification for event {EventId}", @event.Id);
        }
    }
}

