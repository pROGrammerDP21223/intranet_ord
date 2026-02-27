using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class Webhook : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public string Url { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string EventType { get; set; } = string.Empty; // enquiry.created, ticket.updated, etc.

    [MaxLength(50)]
    public string? EntityType { get; set; } // Client, Enquiry, Ticket, etc.

    public int? ClientId { get; set; } // Optional: webhook for specific client

    [MaxLength(100)]
    public string? Secret { get; set; } // For webhook signature verification

    public bool IsActive { get; set; } = true;

    public int RetryCount { get; set; } = 3; // Number of retries on failure

    public int? CreatedBy { get; set; }

    // Navigation property
    [ForeignKey("ClientId")]
    public Client? Client { get; set; }
}

