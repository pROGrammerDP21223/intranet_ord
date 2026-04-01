using System.ComponentModel.DataAnnotations;

namespace backend_net.Domain.Entities;

public class OrdpanelEnquiry
{
    public int Id { get; set; }

    [MaxLength(255)]
    public string? Name { get; set; }

    [MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? ProductName { get; set; }

    [MaxLength(255)]
    public string? ClientName { get; set; }

    /// <summary>Supplier/client listing id from Ordpanel (company page) when form_type is client.</summary>
    [MaxLength(50)]
    public string? ListingClientId { get; set; }

    public string? Message { get; set; }

    [MaxLength(50)]
    public string PageType { get; set; } = "general"; // product, client, general

    [MaxLength(1000)]
    public string? PageUrl { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "New"; // New, Read, Responded

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
