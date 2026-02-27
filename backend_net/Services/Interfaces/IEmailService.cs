namespace backend_net.Services.Interfaces;

public interface IEmailService
{
    System.Threading.Tasks.Task<bool> SendPasswordResetEmailAsync(string email, string name, string resetToken, string resetUrl);
    System.Threading.Tasks.Task<bool> SendEnquiryNotificationToAdminAsync(string adminEmail, string enquiryFullName, string enquiryEmail, string enquiryMobile, string enquiryMessage, string clientName);
    System.Threading.Tasks.Task<bool> SendEnquiryNotificationToClientAsync(string clientEmail, string clientName, string enquiryFullName, string enquiryEmail, string enquiryMobile, string enquiryMessage);
    System.Threading.Tasks.Task<bool> SendEnquiryAutoReplyAsync(string enquiryEmail, string enquiryFullName);
    System.Threading.Tasks.Task<bool> SendTicketCreatedNotificationAsync(string email, string recipientName, string ticketNumber, string ticketTitle, string ticketDescription, string creatorName, string? clientName = null);
    System.Threading.Tasks.Task<bool> SendTicketAssignedNotificationAsync(string email, string recipientName, string ticketNumber, string ticketTitle, string assignedByName);
    System.Threading.Tasks.Task<bool> SendEmailAsync(string toEmail, string subject, string body);
    
    /// <summary>
    /// Sends client creation notification to the specified recipient type (client company, creator, or owner).
    /// </summary>
    System.Threading.Tasks.Task<bool> SendClientCreatedNotificationAsync(string toEmail, string recipientName, string clientCompanyName, string createdByName, string recipientType);
}

