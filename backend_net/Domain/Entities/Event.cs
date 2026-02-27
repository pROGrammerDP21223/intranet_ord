using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class Event : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Column(TypeName = "TEXT")]
    public string? Description { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [MaxLength(50)]
    public string? Location { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Scheduled"; // Scheduled, InProgress, Completed, Cancelled

    [MaxLength(50)]
    public string? Type { get; set; } // Meeting, Appointment, Reminder, Task

    public int? UserId { get; set; } // User who created/owns the event

    public int? ClientId { get; set; } // Optional: if event is related to a client

    public bool IsAllDay { get; set; } = false;

    [MaxLength(50)]
    public string? RecurrencePattern { get; set; } // None, Daily, Weekly, Monthly, Yearly

    public int? RecurrenceInterval { get; set; } // Every N days/weeks/months

    public DateTime? RecurrenceEndDate { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public User? User { get; set; }

    [ForeignKey("ClientId")]
    public Client? Client { get; set; }
}

