using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class UpdateCategoryRequest
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(500)]
    public string? Image { get; set; }

    [Required]
    public int IndustryId { get; set; }
}

