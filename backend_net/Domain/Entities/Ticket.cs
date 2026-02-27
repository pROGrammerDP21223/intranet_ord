using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class Ticket : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string TicketNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "TEXT")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Open"; // Open, InProgress, Resolved, Closed, Cancelled

    [MaxLength(50)]
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Urgent

    [Required]
    public int CreatedBy { get; set; } // UserId who created the ticket

    public int? AssignedTo { get; set; } // UserId who is assigned to work on the ticket (nullable)

    public int? ClientId { get; set; } // Optional: if ticket is related to a specific client

    // Navigation properties
    [ForeignKey("CreatedBy")]
    public User? Creator { get; set; }

    [ForeignKey("AssignedTo")]
    public User? Assignee { get; set; }

    [ForeignKey("ClientId")]
    public Client? Client { get; set; }

    public ICollection<TicketComment>? Comments { get; set; }
}

