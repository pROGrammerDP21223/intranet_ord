using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class CreateWebhookRequest
{
    [Required]
    [MaxLength(255)]
    [Url]
    public string Url { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string EventType { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? EntityType { get; set; }

    public int? ClientId { get; set; }

    [MaxLength(100)]
    public string? Secret { get; set; }

    public bool IsActive { get; set; } = true;

    public int RetryCount { get; set; } = 3;
}

