using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class AttachUserToClientRequest
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int ClientId { get; set; }
}

