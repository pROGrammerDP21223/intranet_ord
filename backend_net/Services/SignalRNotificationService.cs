using backend_net.Domain.Entities;

namespace backend_net.Services;

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
    public System.Threading.Tasks.Task NotifyNewEnquiryAsync(Enquiry enquiry, Client client) => System.Threading.Tasks.Task.CompletedTask;
    public System.Threading.Tasks.Task NotifyTicketUpdateAsync(Ticket ticket, string action) => System.Threading.Tasks.Task.CompletedTask;
    public System.Threading.Tasks.Task NotifyClientApprovalAsync(Client client, int approvedByUserId) => System.Threading.Tasks.Task.CompletedTask;
    public System.Threading.Tasks.Task NotifyTransactionUpdateAsync(Transaction transaction, string action) => System.Threading.Tasks.Task.CompletedTask;
    public System.Threading.Tasks.Task NotifyUserAsync(int userId, string title, string message, string? type = "info") => System.Threading.Tasks.Task.CompletedTask;
    public System.Threading.Tasks.Task NotifyGroupAsync(string groupName, string title, string message, string? type = "info") => System.Threading.Tasks.Task.CompletedTask;
    public System.Threading.Tasks.Task NotifyInternalMessageAsync(Message message) => System.Threading.Tasks.Task.CompletedTask;
    public System.Threading.Tasks.Task NotifyEventUpdateAsync(Event @event, string action) => System.Threading.Tasks.Task.CompletedTask;
}
