using backend_net.Data.Context;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend_net.Services;

public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MessageService> _logger;
    private readonly ISignalRNotificationService? _signalRNotificationService;

    public MessageService(
        ApplicationDbContext context,
        ILogger<MessageService> logger,
        ISignalRNotificationService? signalRNotificationService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _signalRNotificationService = signalRNotificationService;
    }

    public async System.Threading.Tasks.Task<IEnumerable<Message>> GetInboxAsync(int userId)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Where(m => !m.IsDeleted && m.RecipientId == userId && !m.IsDeletedByRecipient)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<IEnumerable<Message>> GetSentMessagesAsync(int userId)
    {
        return await _context.Messages
            .Include(m => m.Recipient)
            .Where(m => !m.IsDeleted && m.SenderId == userId && !m.IsDeletedBySender)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<Message?> GetMessageByIdAsync(int id, int userId)
    {
        var message = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Recipient)
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

        if (message == null)
            return null;

        // Check if user has access (sender or recipient)
        if (message.SenderId != userId && message.RecipientId != userId)
            return null;

        // Mark as read if user is recipient
        if (message.RecipientId == userId && !message.IsRead)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return message;
    }

    public async System.Threading.Tasks.Task<Message> SendMessageAsync(SendMessageRequest request, int senderId)
    {
        var message = new Message
        {
            SenderId = senderId,
            RecipientId = request.RecipientId,
            Subject = request.Subject,
            Content = request.Content,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Messages.AddAsync(message);
        await _context.SaveChangesAsync();

        // Send SignalR notification
        if (_signalRNotificationService != null)
        {
            var messageWithDetails = await GetMessageByIdAsync(message.Id, senderId);
            if (messageWithDetails != null)
            {
                await _signalRNotificationService.NotifyInternalMessageAsync(messageWithDetails);
            }
        }

        return await GetMessageByIdAsync(message.Id, senderId) ?? message;
    }

    public async System.Threading.Tasks.Task<bool> MarkAsReadAsync(int messageId, int userId)
    {
        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == messageId && m.RecipientId == userId && !m.IsDeleted);

        if (message == null)
            return false;

        message.IsRead = true;
        message.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async System.Threading.Tasks.Task<bool> DeleteMessageAsync(int messageId, int userId, bool isSender)
    {
        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == messageId && !m.IsDeleted);

        if (message == null)
            return false;

        // Check access
        if (message.SenderId != userId && message.RecipientId != userId)
            return false;

        if (isSender)
        {
            message.IsDeletedBySender = true;
        }
        else
        {
            message.IsDeletedByRecipient = true;
        }

        // If both deleted, mark as deleted
        if (message.IsDeletedBySender && message.IsDeletedByRecipient)
        {
            message.IsDeleted = true;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async System.Threading.Tasks.Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.Messages
            .CountAsync(m => !m.IsDeleted && m.RecipientId == userId && !m.IsRead && !m.IsDeletedByRecipient);
    }
}

