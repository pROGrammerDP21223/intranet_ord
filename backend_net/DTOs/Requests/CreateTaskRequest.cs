using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class CreateTaskRequest
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(50)]
    public string Priority { get; set; } = "Medium";

    public DateTime? DueDate { get; set; }

    [Required]
    public int AssignedTo { get; set; }

    [MaxLength(50)]
    public string? EntityType { get; set; }

    public int? EntityId { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }
}

