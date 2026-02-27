using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class UpdatePermissionRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;
}

