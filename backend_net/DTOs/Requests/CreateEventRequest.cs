using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class CreateEventRequest
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [MaxLength(50)]
    public string? Location { get; set; }

    [MaxLength(50)]
    public string? Type { get; set; }

    public int? ClientId { get; set; }

    public bool IsAllDay { get; set; } = false;

    [MaxLength(50)]
    public string? RecurrencePattern { get; set; }

    public int? RecurrenceInterval { get; set; }

    public DateTime? RecurrenceEndDate { get; set; }
}

