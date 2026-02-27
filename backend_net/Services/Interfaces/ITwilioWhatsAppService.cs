namespace backend_net.Services.Interfaces;

public interface ITwilioWhatsAppService
{
    Task<bool> SendWhatsAppMessageAsync(string toPhoneNumber, string message);
    Task<bool> SendEnquiryWhatsAppNotificationAsync(string clientPhoneNumber, string clientName, string enquiryFullName, string enquiryEmail, string enquiryMobile, string enquiryMessage);
}

