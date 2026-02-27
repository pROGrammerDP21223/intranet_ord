using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class Category : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "TEXT")]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? Image { get; set; } // Image URL or path

    [Required]
    public int IndustryId { get; set; }

    // Navigation properties
    [ForeignKey("IndustryId")]
    public Industry? Industry { get; set; }

    public ICollection<Product>? Products { get; set; }
}

