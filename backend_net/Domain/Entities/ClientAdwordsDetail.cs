using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class ClientAdwordsDetail : BaseEntity
{
    [Required]
    public int ClientId { get; set; }

    [MaxLength(100)]
    public string? NumberOfKeywords { get; set; }

    [MaxLength(50)]
    public string? Period { get; set; } // "Monthly" or "Quarterly"

    [MaxLength(255)]
    public string? Location { get; set; }

    [Column(TypeName = "TEXT")]
    public string? KeywordsList { get; set; }

    [Column(TypeName = "TEXT")]
    public string? SpecialGuidelines { get; set; }

    // Navigation property
    [ForeignKey("ClientId")]
    public Client? Client { get; set; }
}

