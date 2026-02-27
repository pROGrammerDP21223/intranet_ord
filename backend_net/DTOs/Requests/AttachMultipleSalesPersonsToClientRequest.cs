using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class AttachMultipleSalesPersonsToClientRequest
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one sales person ID is required")]
    public List<int> SalesPersonIds { get; set; } = new List<int>();
}

