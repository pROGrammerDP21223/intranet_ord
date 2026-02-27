using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class Workflow : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(50)]
    public string TriggerEntity { get; set; } = string.Empty; // Client, Enquiry, Ticket, Transaction

    [Required]
    [MaxLength(50)]
    public string TriggerEvent { get; set; } = string.Empty; // Created, Updated, StatusChanged, etc.

    [MaxLength(500)]
    public string? Conditions { get; set; } // JSON string of conditions

    [Required]
    [MaxLength(50)]
    public string ActionType { get; set; } = string.Empty; // CreateTask, SendEmail, SendNotification, etc.

    [Required]
    [Column(TypeName = "TEXT")]
    public string ActionConfig { get; set; } = string.Empty; // JSON string of action configuration

    public bool IsActive { get; set; } = true;

    public int? CreatedBy { get; set; }
}

