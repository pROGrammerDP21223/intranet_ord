using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class AddTicketCommentRequest
{
    [Required]
    public string Comment { get; set; } = string.Empty;

    public bool IsInternal { get; set; } = false; // If true, only staff/admin can see this comment
}

