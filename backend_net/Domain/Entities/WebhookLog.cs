using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class WebhookLog : BaseEntity
{
    [Required]
    public int WebhookId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Url { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string EventType { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "TEXT")]
    public string Payload { get; set; } = string.Empty; // JSON payload sent

    [MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Success, Failed

    public int? StatusCode { get; set; } // HTTP status code

    [Column(TypeName = "TEXT")]
    public string? Response { get; set; } // Response from webhook endpoint

    [Column(TypeName = "TEXT")]
    public string? ErrorMessage { get; set; }

    public DateTime? SentAt { get; set; }

    public int AttemptNumber { get; set; } = 1;

    // Navigation property
    [ForeignKey("WebhookId")]
    public Webhook? Webhook { get; set; }
}

