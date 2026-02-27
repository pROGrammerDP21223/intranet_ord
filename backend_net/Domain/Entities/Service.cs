using System.ComponentModel.DataAnnotations;

namespace backend_net.Domain.Entities;

public class Service : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string ServiceType { get; set; } = string.Empty; // e.g., "domain-hosting", "website-design-development"

    [Required]
    [MaxLength(255)]
    public string ServiceName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Category { get; set; } // e.g., "Domain & Hosting", "Web Design", "SEO", "Additional Services"

    public bool IsActive { get; set; } = true;

    public int? SortOrder { get; set; }
}

