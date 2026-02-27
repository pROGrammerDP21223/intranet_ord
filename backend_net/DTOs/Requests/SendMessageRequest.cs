using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class SendMessageRequest
{
    [Required]
    public int RecipientId { get; set; }

    [MaxLength(255)]
    public string? Subject { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;
}

