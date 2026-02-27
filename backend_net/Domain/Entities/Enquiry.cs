using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class Enquiry : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string MobileNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string EmailId { get; set; } = string.Empty;

    [Required]
    public int ClientId { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "New"; // New, In Progress, Resolved, Closed

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(100)]
    public string? Source { get; set; } // Website, Phone, Email, etc.

    [MaxLength(500)]
    public string? ReferrerUrl { get; set; } // Where the enquiry came from

    [Column(TypeName = "NVARCHAR(MAX)")]
    public string? RawPayload { get; set; } // JSON payload for additional fields

    public DateTime? ResolvedAt { get; set; }

    [ForeignKey("ClientId")]
    public Client? Client { get; set; }
}

