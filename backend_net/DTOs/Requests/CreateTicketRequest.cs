using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class CreateTicketRequest
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Urgent

    public int? ClientId { get; set; } // Optional: if ticket is related to a specific client
}

