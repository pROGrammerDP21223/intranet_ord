using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class AttachMultipleProductsToClientRequest
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one product ID is required")]
    public List<int> ProductIds { get; set; } = new List<int>();
}

