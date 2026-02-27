using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class User : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    [MaxLength(100)]
    public string? LastUpdatedBy { get; set; }

    // Role relationship
    public int? RoleId { get; set; }

    [ForeignKey("RoleId")]
    public Role? Role { get; set; }

    // Two-Factor Authentication
    [MaxLength(255)]
    public string? TwoFactorSecret { get; set; } // TOTP secret key

    public bool TwoFactorEnabled { get; set; } = false;

    public bool TwoFactorEmailEnabled { get; set; } = false;

    [MaxLength(500)]
    public string? BackupCodes { get; set; } // JSON array of backup codes

    // Navigation properties
    [NotMapped]
    public User? Creator { get; set; }

    [NotMapped]
    public User? Updater { get; set; }
}

