using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class CreateProductRequest
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [MaxLength(500)]
    public string MainImage { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }

    public List<string>? AdditionalImages { get; set; } // List of image URLs
}

