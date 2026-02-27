using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class UpdateIndustryRequest
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(500)]
    public string? Image { get; set; }

    public bool TopIndustry { get; set; } = false;

    public bool BannerIndustry { get; set; } = false;
}

