using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class UpdateRoleRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }
}

