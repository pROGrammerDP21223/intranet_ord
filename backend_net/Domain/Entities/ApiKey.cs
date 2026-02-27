using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class ApiKey : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public string Key { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public int ClientId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? ExpiresAt { get; set; }

    [MaxLength(500)]
    public string? AllowedOrigins { get; set; } // Comma-separated list of allowed origins

    [ForeignKey("ClientId")]
    public Client? Client { get; set; }
}

