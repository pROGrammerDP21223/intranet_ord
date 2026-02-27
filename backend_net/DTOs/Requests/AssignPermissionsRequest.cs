using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class AssignPermissionsRequest
{
    [Required]
    public List<int> PermissionIds { get; set; } = new();
}

