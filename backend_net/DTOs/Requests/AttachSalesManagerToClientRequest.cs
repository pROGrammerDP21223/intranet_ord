using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class AttachSalesManagerToClientRequest
{
    [Required]
    public int SalesManagerId { get; set; }

    [Required]
    public int ClientId { get; set; }
}
