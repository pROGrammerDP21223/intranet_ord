using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class CreateApiKeyRequest
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime? ExpiresAt { get; set; }

    [MaxLength(500)]
    public string? AllowedOrigins { get; set; }
}

