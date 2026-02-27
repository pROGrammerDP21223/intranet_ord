using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class Message : BaseEntity
{
    [Required]
    public int SenderId { get; set; }

    [Required]
    public int RecipientId { get; set; }

    [Required]
    [Column(TypeName = "TEXT")]
    public string Content { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Subject { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime? ReadAt { get; set; }

    public bool IsDeletedBySender { get; set; } = false;

    public bool IsDeletedByRecipient { get; set; } = false;

    // Navigation properties
    [ForeignKey("SenderId")]
    public User? Sender { get; set; }

    [ForeignKey("RecipientId")]
    public User? Recipient { get; set; }
}

