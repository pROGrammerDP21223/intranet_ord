using backend_net.Common.Results;
using backend_net.Domain.Entities;
using backend_net.Services.Interfaces;
using backend_net.Services.Notifications.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace backend_net.Services.Notifications.Handlers;

/// <summary>
/// Email notification handler implementing Strategy pattern
/// Handles all email notifications for enquiries
/// </summary>
public class EmailNotificationHandler : INotificationHandler
{
    public string NotificationType => "Email";

    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailNotificationHandler> _logger;

    public EmailNotificationHandler(
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<EmailNotificationHandler> logger)
    {
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result> SendAsync(Enquiry enquiry, Client client, CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = new List<Task<bool>>();
            var enquiryMessage = ExtractMessageFromPayload(enquiry.RawPayload);

            // 1. Auto-reply to enquirer (first)
            tasks.Add(_emailService.SendEnquiryAutoReplyAsync(
                enquiry.EmailId,
                enquiry.FullName));

            // 2. Send email to client - to client Email and EnquiryEmail (if present)
            var clientEmails = GetClientEmailsToNotify(client);
            foreach (var clientEmail in clientEmails)
            {
                tasks.Add(_emailService.SendEnquiryNotificationToClientAsync(
                    clientEmail,
                    client.CompanyName,
                    enquiry.FullName,
                    enquiry.EmailId,
                    enquiry.MobileNumber,
                    enquiryMessage));
            }

            // 3. Send email to admin
            var adminEmail = _configuration["Email:AdminEmail"] ?? _configuration["Email:FromEmail"] ?? "";
            if (!string.IsNullOrWhiteSpace(adminEmail))
            {
                tasks.Add(_emailService.SendEnquiryNotificationToAdminAsync(
                    adminEmail,
                    enquiry.FullName,
                    enquiry.EmailId,
                    enquiry.MobileNumber,
                    enquiryMessage,
                    client.CompanyName));
            }

            var results = await System.Threading.Tasks.Task.WhenAll(tasks);
            var allSucceeded = results.All(r => r);

            if (allSucceeded)
            {
                _logger.LogInformation("All email notifications sent successfully for enquiry {EnquiryId}", enquiry.Id);
                return Result.Success();
            }

            _logger.LogWarning("Some email notifications failed for enquiry {EnquiryId}", enquiry.Id);
            return Result.Failure("Some email notifications failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email notifications for enquiry {EnquiryId}", enquiry.Id);
            return Result.Failure($"Error sending email notifications: {ex.Message}");
        }
    }

    /// <summary>
    /// Returns all client email addresses to notify about an enquiry:
    /// - Client's main email (Email)
    /// - Client's enquiry email (EnquiryEmail) if present and different from main
    /// </summary>
    private IEnumerable<string> GetClientEmailsToNotify(Client client)
    {
        var emails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(client.Email))
            emails.Add(client.Email.Trim());

        if (!string.IsNullOrWhiteSpace(client.EnquiryEmail))
        {
            var enquiryEmail = client.EnquiryEmail.Trim();
            if (!emails.Contains(enquiryEmail))
                emails.Add(enquiryEmail);
        }

        return emails;
    }

    private string ExtractMessageFromPayload(string? rawPayload)
    {
        if (string.IsNullOrWhiteSpace(rawPayload))
            return "No message provided";

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(rawPayload);
            var root = doc.RootElement;

            if (root.TryGetProperty("message", out var messageElement))
                return messageElement.GetString() ?? "No message provided";

            if (root.TryGetProperty("Message", out var messageElement2))
                return messageElement2.GetString() ?? "No message provided";

            return "No message provided";
        }
        catch
        {
            return "No message provided";
        }
    }
}

