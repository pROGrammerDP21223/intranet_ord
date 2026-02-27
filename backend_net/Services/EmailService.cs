using System.Net;
using System.Net.Mail;
using backend_net.Constants;
using backend_net.Services.Interfaces;

namespace backend_net.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly IEmailTemplateService? _emailTemplateService;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger, IEmailTemplateService? emailTemplateService = null)
    {
        _configuration = configuration;
        _logger = logger;
        _emailTemplateService = emailTemplateService;
    }

    private static Dictionary<string, string> WithYear(Dictionary<string, string> vars)
    {
        var result = new Dictionary<string, string>(vars) { ["year"] = DateTime.Now.Year.ToString() };
        return result;
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string name, string resetToken, string resetUrl)
    {
        try
        {
            var fullResetUrl = $"{resetUrl}?token={resetToken}";
            var (subject, body) = await GetRenderedTemplateAsync(EmailTemplateTypes.PasswordReset, new Dictionary<string, string>
            {
                ["name"] = name,
                ["fullResetUrl"] = fullResetUrl
            });
            if (subject == null) // fallback
            {
                subject = "Password Reset Request - One Rank Digital";
                body = $@"<!DOCTYPE html><html><head><style>body{{font-family:Arial,sans-serif;}}.container{{max-width:600px;margin:0 auto;padding:20px;}}.header{{background:linear-gradient(135deg,#0b51b7 0%,#7081b9 100%);color:white;padding:20px;text-align:center;}}.content{{background:#f9f9f9;padding:30px;}}.button{{display:inline-block;padding:12px 30px;background:#0b51b7;color:white;text-decoration:none;border-radius:5px;}}</style></head><body><div class=""container""><div class=""header""><h2>Password Reset Request</h2></div><div class=""content""><p>Hello {name},</p><p>We received a request to reset your password.</p><p><a href=""{fullResetUrl}"" class=""button"">Reset Password</a></p><p>Or copy: {fullResetUrl}</p><p>This link expires in 1 hour.</p></div><div class=""footer""><p>&copy; {DateTime.Now.Year} One Rank Digital.</p></div></div></body></html>";
            }

            return await SendEmailAsync(email, subject, body!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send password reset email to {email}");
            return false;
        }
    }

    private async Task<(string? Subject, string? Body)> GetRenderedTemplateAsync(string templateType, Dictionary<string, string> variables)
    {
        if (_emailTemplateService == null) return (null, null);
        var template = await _emailTemplateService.GetByTypeAsync(templateType);
        if (template == null) return (null, null);
        var (subject, body) = await _emailTemplateService.RenderWithSubjectAsync(template, WithYear(variables));
        return (subject, body);
    }

    public async System.Threading.Tasks.Task<bool> SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["Email:SmtpUsername"] ?? "";
            var smtpPassword = _configuration["Email:SmtpPassword"] ?? "";
            var fromEmail = _configuration["Email:FromEmail"] ?? smtpUsername;
            var fromName = _configuration["Email:FromName"] ?? "One Rank Digital Admin";

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);
            await client.SendMailAsync(message);
            _logger.LogInformation($"Email sent successfully to {toEmail}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email to {toEmail}");
            return false;
        }
    }

    public async Task<bool> SendEnquiryNotificationToAdminAsync(string adminEmail, string enquiryFullName, string enquiryEmail, string enquiryMobile, string enquiryMessage, string clientName)
    {
        var (subject, body) = await GetRenderedTemplateAsync(EmailTemplateTypes.EnquiryNotificationToAdmin, new Dictionary<string, string>
        {
            ["clientName"] = clientName,
            ["enquiryFullName"] = enquiryFullName,
            ["enquiryEmail"] = enquiryEmail,
            ["enquiryMobile"] = enquiryMobile,
            ["enquiryMessage"] = enquiryMessage
        });
        if (subject == null) { subject = "New Enquiry Received"; body = $@"<p>Hello Admin,</p><p>New enquiry from {enquiryFullName} ({enquiryEmail}) for client {clientName}. Message: {enquiryMessage}</p>"; }
        return await SendEmailAsync(adminEmail, subject, body!);
    }

    public async Task<bool> SendEnquiryNotificationToClientAsync(string clientEmail, string clientName, string enquiryFullName, string enquiryEmail, string enquiryMobile, string enquiryMessage)
    {
        var (subject, body) = await GetRenderedTemplateAsync(EmailTemplateTypes.EnquiryNotificationToClient, new Dictionary<string, string>
        {
            ["clientName"] = clientName,
            ["enquiryFullName"] = enquiryFullName,
            ["enquiryEmail"] = enquiryEmail,
            ["enquiryMobile"] = enquiryMobile,
            ["enquiryMessage"] = enquiryMessage
        });
        if (subject == null) { subject = "New Enquiry Received for Your Business"; body = $@"<p>Hello {clientName},</p><p>New enquiry from {enquiryFullName} ({enquiryEmail}). Message: {enquiryMessage}</p>"; }
        return await SendEmailAsync(clientEmail, subject, body!);
    }

    public async Task<bool> SendEnquiryAutoReplyAsync(string enquiryEmail, string enquiryFullName)
    {
        var (subject, body) = await GetRenderedTemplateAsync(EmailTemplateTypes.EnquiryAutoReply, new Dictionary<string, string>
        {
            ["enquiryFullName"] = enquiryFullName
        });
        if (subject == null) { subject = "Thank You for Your Enquiry"; body = $@"<p>Dear {enquiryFullName},</p><p>Thank you for contacting us. We have received your enquiry and our team will review it shortly.</p><p>Best regards,<br/>One Rank Digital Team</p>"; }
        return await SendEmailAsync(enquiryEmail, subject, body!);
    }

    public async Task<bool> SendTicketCreatedNotificationAsync(string email, string recipientName, string ticketNumber, string ticketTitle, string ticketDescription, string creatorName, string? clientName = null)
    {
        var clientInfo = !string.IsNullOrEmpty(clientName) ? $"<p><strong>Client:</strong> {clientName}</p>" : "";
        var (subject, body) = await GetRenderedTemplateAsync(EmailTemplateTypes.TicketCreated, new Dictionary<string, string>
        {
            ["recipientName"] = recipientName,
            ["ticketNumber"] = ticketNumber,
            ["ticketTitle"] = ticketTitle,
            ["clientInfo"] = clientInfo,
            ["creatorName"] = creatorName,
            ["ticketDescription"] = ticketDescription
        });
        if (subject == null) { subject = $"New Ticket Created: {ticketNumber}"; body = $@"<p>Hello {recipientName},</p><p>New ticket {ticketNumber}: {ticketTitle} by {creatorName}. {ticketDescription}</p>"; }
        return await SendEmailAsync(email, subject, body!);
    }

    public async Task<bool> SendTicketAssignedNotificationAsync(string email, string recipientName, string ticketNumber, string ticketTitle, string assignedByName)
    {
        var (subject, body) = await GetRenderedTemplateAsync(EmailTemplateTypes.TicketAssigned, new Dictionary<string, string>
        {
            ["recipientName"] = recipientName,
            ["ticketNumber"] = ticketNumber,
            ["ticketTitle"] = ticketTitle,
            ["assignedByName"] = assignedByName
        });
        if (subject == null) { subject = $"Ticket Assigned to You: {ticketNumber}"; body = $@"<p>Hello {recipientName},</p><p>Ticket {ticketNumber} ({ticketTitle}) has been assigned to you by {assignedByName}.</p>"; }
        return await SendEmailAsync(email, subject, body!);
    }

    public async Task<bool> SendClientCreatedNotificationAsync(string toEmail, string recipientName, string clientCompanyName, string createdByName, string recipientType)
    {
        string templateType;
        Dictionary<string, string> variables;
        string fallbackSubject;
        string fallbackBody;

        switch (recipientType.ToLowerInvariant())
        {
            case "client":
                templateType = EmailTemplateTypes.ClientCreatedClient;
                variables = new Dictionary<string, string> { ["recipientName"] = recipientName, ["clientCompanyName"] = clientCompanyName, ["createdByName"] = createdByName };
                fallbackSubject = "Welcome to One Rank Digital - Your Client Profile Has Been Created";
                fallbackBody = $"<p>Hello {recipientName},</p><p>Your client profile for {clientCompanyName} has been created by {createdByName}.</p>";
                break;
            case "creator":
                templateType = EmailTemplateTypes.ClientCreatedCreator;
                variables = new Dictionary<string, string> { ["recipientName"] = recipientName, ["clientCompanyName"] = clientCompanyName };
                fallbackSubject = $"Client Created Successfully: {clientCompanyName}";
                fallbackBody = $"<p>Hello {recipientName},</p><p>You have successfully created client {clientCompanyName}.</p>";
                break;
            default:
                templateType = EmailTemplateTypes.ClientCreatedOwner;
                variables = new Dictionary<string, string> { ["recipientName"] = recipientName, ["clientCompanyName"] = clientCompanyName, ["createdByName"] = createdByName };
                fallbackSubject = $"New Client Created: {clientCompanyName}";
                fallbackBody = $"<p>Hello {recipientName},</p><p>New client {clientCompanyName} created by {createdByName}.</p>";
                break;
        }

        var (subject, body) = await GetRenderedTemplateAsync(templateType, variables);
        if (subject == null) { subject = fallbackSubject; body = fallbackBody; }
        return await SendEmailAsync(toEmail, subject, body!);
    }
}

