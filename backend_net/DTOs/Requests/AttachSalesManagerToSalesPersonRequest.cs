using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class AttachSalesManagerToSalesPersonRequest
{
    [Required]
    public int SalesManagerId { get; set; }

    [Required]
    public int SalesPersonId { get; set; }
}

