using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class CreateWorkflowRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(50)]
    public string TriggerEntity { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string TriggerEvent { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Conditions { get; set; }

    [Required]
    [MaxLength(50)]
    public string ActionType { get; set; } = string.Empty;

    [Required]
    public string ActionConfig { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

