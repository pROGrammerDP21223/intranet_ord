using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class Task : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Column(TypeName = "TEXT")]
    public string? Description { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Cancelled

    [Required]
    [MaxLength(50)]
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Urgent

    public DateTime? DueDate { get; set; }

    public DateTime? CompletedAt { get; set; }

    [Required]
    public int AssignedTo { get; set; }

    public int? CreatedBy { get; set; }

    [MaxLength(50)]
    public string? EntityType { get; set; } // Client, Enquiry, Ticket, etc.

    public int? EntityId { get; set; } // ID of related entity

    [MaxLength(100)]
    public string? Category { get; set; }

    // Navigation properties
    [ForeignKey("AssignedTo")]
    public User? Assignee { get; set; }

    [ForeignKey("CreatedBy")]
    public User? Creator { get; set; }
}

