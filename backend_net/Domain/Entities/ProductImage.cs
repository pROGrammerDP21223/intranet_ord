using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class ProductImage : BaseEntity
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty; // Image URL or path

    public int? SortOrder { get; set; } // For ordering multiple images

    // Navigation properties
    [ForeignKey("ProductId")]
    public Product? Product { get; set; }
}

