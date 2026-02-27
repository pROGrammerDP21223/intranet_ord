using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class UpdateWorkflowRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? TriggerEntity { get; set; }

    [MaxLength(50)]
    public string? TriggerEvent { get; set; }

    [MaxLength(500)]
    public string? Conditions { get; set; }

    [MaxLength(50)]
    public string? ActionType { get; set; }

    public string? ActionConfig { get; set; }

    public bool? IsActive { get; set; }
}

