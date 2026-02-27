using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class AssignTicketRequest
{
    [Required]
    public int AssignedToUserId { get; set; }
}

