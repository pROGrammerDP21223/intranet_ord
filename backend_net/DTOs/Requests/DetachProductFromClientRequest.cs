using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class DetachProductFromClientRequest
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    public int ProductId { get; set; }
}

