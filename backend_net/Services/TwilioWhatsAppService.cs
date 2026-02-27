using backend_net.Services.Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace backend_net.Services;

public class TwilioWhatsAppService : ITwilioWhatsAppService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioWhatsAppService> _logger;
    private readonly string? _accountSid;
    private readonly string? _authToken;
    private readonly string? _whatsAppFromNumber;

    public TwilioWhatsAppService(IConfiguration configuration, ILogger<TwilioWhatsAppService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _accountSid = _configuration["Twilio:AccountSid"];
        _authToken = _configuration["Twilio:AuthToken"];
        _whatsAppFromNumber = _configuration["Twilio:WhatsAppFromNumber"];

        // Initialize Twilio client if credentials are available
        if (!string.IsNullOrWhiteSpace(_accountSid) && !string.IsNullOrWhiteSpace(_authToken))
        {
            TwilioClient.Init(_accountSid, _authToken);
        }
    }

    public async Task<bool> SendWhatsAppMessageAsync(string toPhoneNumber, string message)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_accountSid) || string.IsNullOrWhiteSpace(_authToken) || string.IsNullOrWhiteSpace(_whatsAppFromNumber))
            {
                _logger.LogWarning("Twilio credentials not configured. WhatsApp message not sent.");
                return false;
            }

            // Format phone number (ensure it starts with whatsapp:)
            var formattedNumber = FormatPhoneNumber(toPhoneNumber);
            if (string.IsNullOrWhiteSpace(formattedNumber))
            {
                _logger.LogWarning($"Invalid phone number format: {toPhoneNumber}");
                return false;
            }

            var formattedFromNumber = FormatWhatsAppFromNumber(_whatsAppFromNumber);
            if (string.IsNullOrWhiteSpace(formattedFromNumber))
            {
                _logger.LogWarning($"Invalid WhatsApp from number format: {_whatsAppFromNumber}");
                return false;
            }

            var messageResource = await MessageResource.CreateAsync(
                from: new PhoneNumber(formattedFromNumber),
                to: new PhoneNumber(formattedNumber),
                body: message
            );

            _logger.LogInformation($"WhatsApp message sent successfully to {formattedNumber}. SID: {messageResource.Sid}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send WhatsApp message to {toPhoneNumber}");
            return false;
        }
    }

    public async Task<bool> SendEnquiryWhatsAppNotificationAsync(string clientPhoneNumber, string clientName, string enquiryFullName, string enquiryEmail, string enquiryMobile, string enquiryMessage)
    {
        var message = $@"🔔 *New Enquiry Received*

Hello {clientName},

A new enquiry has been received for your business:

*Enquirer Details:*
👤 Name: {enquiryFullName}
📧 Email: {enquiryEmail}
📱 Mobile: {enquiryMobile}

*Message:*
{enquiryMessage}

We will follow up with the enquirer and keep you informed.

Thank you,
One Rank Digital Team";

        return await SendWhatsAppMessageAsync(clientPhoneNumber, message);
    }

    private string? FormatPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return null;

        // Remove all non-digit characters except +
        var cleaned = new string(phoneNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());

        // If it doesn't start with +, add it
        if (!cleaned.StartsWith("+"))
        {
            // Assume it's an Indian number if it starts with 0, remove 0 and add +91
            if (cleaned.StartsWith("0"))
            {
                cleaned = "+91" + cleaned.Substring(1);
            }
            else if (cleaned.Length == 10)
            {
                // Assume it's a 10-digit Indian number
                cleaned = "+91" + cleaned;
            }
            else
            {
                cleaned = "+" + cleaned;
            }
        }

        // Add whatsapp: prefix for Twilio
        return $"whatsapp:{cleaned}";
    }

    private string? FormatWhatsAppFromNumber(string fromNumber)
    {
        if (string.IsNullOrWhiteSpace(fromNumber))
            return null;

        if (fromNumber.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase))
            return fromNumber;

        var cleaned = new string(fromNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());
        if (string.IsNullOrWhiteSpace(cleaned))
            return null;

        if (!cleaned.StartsWith("+"))
        {
            if (cleaned.StartsWith("0"))
            {
                cleaned = "+91" + cleaned.Substring(1);
            }
            else if (cleaned.Length == 10)
            {
                cleaned = "+91" + cleaned;
            }
            else
            {
                cleaned = "+" + cleaned;
            }
        }

        return $"whatsapp:{cleaned}";
    }
}

