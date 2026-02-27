using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class AttachSalesPersonToClientRequest
{
    [Required]
    public int SalesPersonId { get; set; }

    [Required]
    public int ClientId { get; set; }
}

