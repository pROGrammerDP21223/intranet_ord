using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class EmailTemplate : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "TEXT")]
    public string Body { get; set; } = string.Empty;

    [MaxLength(50)]
    public string TemplateType { get; set; } = string.Empty; // enquiry_notification, ticket_created, etc.

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Variables { get; set; } // JSON string of available variables
}

