using backend_net.Common.Results;
using backend_net.Domain.Entities;
using backend_net.Services.Interfaces;
using backend_net.Services.Notifications.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend_net.Services.Notifications.Handlers;

/// <summary>
/// WhatsApp notification handler implementing Strategy pattern
/// Handles WhatsApp notifications for enquiries
/// </summary>
public class WhatsAppNotificationHandler : INotificationHandler
{
    public string NotificationType => "WhatsApp";

    private readonly ITwilioWhatsAppService _whatsAppService;
    private readonly ILogger<WhatsAppNotificationHandler> _logger;

    public WhatsAppNotificationHandler(
        ITwilioWhatsAppService whatsAppService,
        ILogger<WhatsAppNotificationHandler> logger)
    {
        _whatsAppService = whatsAppService;
        _logger = logger;
    }

    public async Task<Result> SendAsync(Enquiry enquiry, Client client, CancellationToken cancellationToken = default)
    {
        try
        {
            // Only send if WhatsApp service is enabled for this client
            if (!client.UseWhatsAppService)
            {
                _logger.LogDebug("WhatsApp service disabled for client {ClientId}", client.Id);
                return Result.Success(); // Not an error, just disabled
            }

            var clientWhatsAppNumber = DetermineWhatsAppNumber(client);
            if (string.IsNullOrWhiteSpace(clientWhatsAppNumber))
            {
                _logger.LogWarning("No WhatsApp number available for client {ClientId}", client.Id);
                return Result.Failure("No WhatsApp number available for client");
            }

            var enquiryDetails = ExtractDetailsFromPayload(enquiry.RawPayload);
            var success = await _whatsAppService.SendEnquiryWhatsAppNotificationAsync(
                clientWhatsAppNumber,
                client.CompanyName,
                enquiry.FullName,
                enquiry.EmailId,
                enquiry.MobileNumber,
                enquiryDetails);

            if (success)
            {
                _logger.LogInformation("WhatsApp notification sent successfully for enquiry {EnquiryId}", enquiry.Id);
                return Result.Success();
            }

            return Result.Failure("Failed to send WhatsApp notification");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WhatsApp notification for enquiry {EnquiryId}", enquiry.Id);
            return Result.Failure($"Error sending WhatsApp notification: {ex.Message}");
        }
    }

    private string? DetermineWhatsAppNumber(Client client)
    {
        return client.WhatsAppSameAsMobile
            ? client.Phone
            : (client.WhatsAppNumber ?? client.Phone);
    }

    private string ExtractDetailsFromPayload(string? rawPayload)
    {
        if (string.IsNullOrWhiteSpace(rawPayload))
            return "No additional details provided";

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(rawPayload);
            var root = doc.RootElement;

            if (root.ValueKind != System.Text.Json.JsonValueKind.Object)
            {
                return "No additional details provided";
            }

            var entries = new List<(string Key, string Value)>();
            foreach (var property in root.EnumerateObject())
            {
                var value = RenderJsonValue(property.Value);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    entries.Add((property.Name, value));
                }
            }

            if (!entries.Any())
            {
                return "No additional details provided";
            }

            var orderedEntries = entries
                .OrderBy(e => IsMessageKey(e.Key) ? 0 : 1)
                .ThenBy(e => e.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return string.Join(Environment.NewLine, orderedEntries.Select(e =>
                $"{FormatLabel(e.Key)}: {e.Value}"));
        }
        catch
        {
            return "No additional details provided";
        }
    }

    private static bool IsMessageKey(string key)
    {
        var normalized = key.Replace("_", string.Empty).Trim().ToLowerInvariant();
        return normalized == "message" || normalized == "enquirymessage";
    }

    private static string FormatLabel(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return "Field";

        var cleaned = key.Replace("_", " ").Trim();
        var label = System.Text.RegularExpressions.Regex
            .Replace(cleaned, "([a-z])([A-Z])", "$1 $2");
        return System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(label);
    }

    private static string RenderJsonValue(System.Text.Json.JsonElement element)
    {
        switch (element.ValueKind)
        {
            case System.Text.Json.JsonValueKind.String:
                return element.GetString() ?? string.Empty;
            case System.Text.Json.JsonValueKind.Number:
                return element.GetRawText();
            case System.Text.Json.JsonValueKind.True:
                return "Yes";
            case System.Text.Json.JsonValueKind.False:
                return "No";
            case System.Text.Json.JsonValueKind.Array:
                var items = element.EnumerateArray()
                    .Select(RenderJsonValue)
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .ToList();
                return items.Count > 0 ? string.Join(", ", items) : string.Empty;
            case System.Text.Json.JsonValueKind.Object:
                var props = element.EnumerateObject()
                    .Select(p => $"{FormatLabel(p.Name)}: {RenderJsonValue(p.Value)}")
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .ToList();
                return props.Count > 0 ? string.Join("; ", props) : string.Empty;
            default:
                return string.Empty;
        }
    }
}

