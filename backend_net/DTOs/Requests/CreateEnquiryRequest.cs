using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace backend_net.DTOs.Requests;

public class CreateEnquiryRequest
{
    [Required(ErrorMessage = "FullName is required")]
    [MaxLength(200, ErrorMessage = "FullName cannot exceed 200 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "MobileNumber is required")]
    [MaxLength(20, ErrorMessage = "MobileNumber cannot exceed 20 characters")]
    [RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "Invalid mobile number format")]
    public string MobileNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "EmailId is required")]
    [MaxLength(255, ErrorMessage = "EmailId cannot exceed 255 characters")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string EmailId { get; set; } = string.Empty;

    [Required(ErrorMessage = "ClientId is required")]
    public int ClientId { get; set; }

    [MaxLength(100, ErrorMessage = "Source cannot exceed 100 characters")]
    public string? Source { get; set; }

    [MaxLength(500, ErrorMessage = "Referrer URL cannot exceed 500 characters")]
    public string? ReferrerUrl { get; set; }

    // Raw payload for additional fields (JSON)
    public JsonElement? RawPayload { get; set; }

    // Security fields for website source
    public string? CaptchaToken { get; set; } // For CAPTCHA validation
    public string? CsrfToken { get; set; } // For CSRF validation
    public string? Honeypot { get; set; } // Honeypot field (should be empty)
}

