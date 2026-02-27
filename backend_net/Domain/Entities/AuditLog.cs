using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class AuditLog : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty; // Client, Enquiry, Transaction, etc.

    [Required]
    public int EntityId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty; // Create, Update, Delete

    public int? UserId { get; set; }

    [MaxLength(255)]
    public string? UserName { get; set; }

    [MaxLength(100)]
    public string? UserEmail { get; set; }

    [Column(TypeName = "TEXT")]
    public string? OldValues { get; set; } // JSON string of old values

    [Column(TypeName = "TEXT")]
    public string? NewValues { get; set; } // JSON string of new values

    [MaxLength(500)]
    public string? Changes { get; set; } // Summary of changes

    [MaxLength(500)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }
}

