using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class ClientSeoDetail : BaseEntity
{
    [Required]
    public int ClientId { get; set; }

    [MaxLength(50)]
    public string? KeywordRange { get; set; } // "upto-25", "25-50", "75-100"

    [MaxLength(50)]
    public string? Location { get; set; } // "state-wise", "india", "country-wise", "global"

    [Column(TypeName = "TEXT")]
    public string? KeywordsList { get; set; } // List of keywords

    // Navigation property
    [ForeignKey("ClientId")]
    public Client? Client { get; set; }
}

