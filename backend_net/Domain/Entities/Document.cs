using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class Document : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FileType { get; set; } = string.Empty; // pdf, docx, xlsx, etc.

    [Required]
    public long FileSize { get; set; } // in bytes

    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty; // Client, Enquiry, Transaction, etc.

    [Required]
    public int EntityId { get; set; }

    [MaxLength(255)]
    public string? Category { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; } // Comma-separated tags

    public int? UploadedBy { get; set; }

    [MaxLength(255)]
    public string? UploadedByName { get; set; }

    public int Version { get; set; } = 1; // For versioning

    public int? ParentDocumentId { get; set; } // For versioning - points to original document

    [ForeignKey("ParentDocumentId")]
    public Document? ParentDocument { get; set; }
}

