using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class AttachOwnerToClientRequest
{
    [Required]
    public int OwnerId { get; set; }

    [Required]
    public int ClientId { get; set; }
}
