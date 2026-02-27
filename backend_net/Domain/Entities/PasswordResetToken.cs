using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class PasswordResetToken : BaseEntity
{
    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Token { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; } = false;

    [Required]
    public DateTime UsedAt { get; set; } = DateTime.MinValue;

    // Navigation property
    [ForeignKey("UserId")]
    public User? User { get; set; }
}

