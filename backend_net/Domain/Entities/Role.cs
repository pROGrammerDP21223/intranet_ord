using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class Role : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    // Navigation properties
    [NotMapped]
    public ICollection<User>? Users { get; set; }

    public ICollection<RolePermission>? RolePermissions { get; set; }

    [NotMapped]
    public ICollection<Permission>? Permissions { get; set; }
}


