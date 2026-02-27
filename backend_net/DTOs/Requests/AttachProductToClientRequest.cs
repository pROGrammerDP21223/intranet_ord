using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class AttachProductToClientRequest
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    public int ProductId { get; set; }
}

