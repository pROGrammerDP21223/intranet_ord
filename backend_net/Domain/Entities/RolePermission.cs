using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class RolePermission : BaseEntity
{
    [Required]
    public int RoleId { get; set; }

    [Required]
    public int PermissionId { get; set; }

    // Navigation properties
    [ForeignKey("RoleId")]
    public Role? Role { get; set; }

    [ForeignKey("PermissionId")]
    public Permission? Permission { get; set; }
}

