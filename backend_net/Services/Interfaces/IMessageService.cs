using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;

namespace backend_net.Services.Interfaces;

public interface IMessageService
{
    System.Threading.Tasks.Task<IEnumerable<Message>> GetInboxAsync(int userId);
    System.Threading.Tasks.Task<IEnumerable<Message>> GetSentMessagesAsync(int userId);
    System.Threading.Tasks.Task<Message?> GetMessageByIdAsync(int id, int userId);
    System.Threading.Tasks.Task<Message> SendMessageAsync(SendMessageRequest request, int senderId);
    System.Threading.Tasks.Task<bool> MarkAsReadAsync(int messageId, int userId);
    System.Threading.Tasks.Task<bool> DeleteMessageAsync(int messageId, int userId, bool isSender);
    System.Threading.Tasks.Task<int> GetUnreadCountAsync(int userId);
}

