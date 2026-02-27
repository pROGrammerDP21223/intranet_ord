using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class UpdateTicketRequest
{
    [MaxLength(255)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Status { get; set; } // Open, InProgress, Resolved, Closed, Cancelled

    [MaxLength(50)]
    public string? Priority { get; set; } // Low, Medium, High, Urgent
}

