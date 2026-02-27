using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class Product : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "TEXT")]
    public string? Description { get; set; }

    [Required]
    [MaxLength(500)]
    public string MainImage { get; set; } = string.Empty; // Main image URL or path

    [Required]
    public int CategoryId { get; set; }

    // Navigation properties
    [ForeignKey("CategoryId")]
    public Category? Category { get; set; }

    public ICollection<ProductImage>? ProductImages { get; set; }
    public ICollection<ClientProduct>? ClientProducts { get; set; }
}

