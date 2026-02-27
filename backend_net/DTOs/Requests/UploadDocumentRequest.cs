using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class UploadDocumentRequest
{
    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    public int EntityId { get; set; }

    [MaxLength(255)]
    public string? Category { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }
}

