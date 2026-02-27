using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class UpdateWebhookRequest
{
    [MaxLength(255)]
    [Url]
    public string? Url { get; set; }

    [MaxLength(50)]
    public string? EventType { get; set; }

    [MaxLength(50)]
    public string? EntityType { get; set; }

    public int? ClientId { get; set; }

    [MaxLength(100)]
    public string? Secret { get; set; }

    public bool? IsActive { get; set; }

    public int? RetryCount { get; set; }
}

