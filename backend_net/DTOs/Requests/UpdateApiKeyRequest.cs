using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class UpdateApiKeyRequest
{
    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime? ExpiresAt { get; set; }

    [MaxLength(500)]
    public string? AllowedOrigins { get; set; }

    public bool? IsActive { get; set; }
}

