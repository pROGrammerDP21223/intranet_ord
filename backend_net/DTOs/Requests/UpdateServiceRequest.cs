using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class UpdateServiceRequest
{
    [Required]
    [MaxLength(100)]
    public string ServiceType { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string ServiceName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Category { get; set; }

    public bool? IsActive { get; set; }

    public int? SortOrder { get; set; }
}

