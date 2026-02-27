using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class UpdateTaskRequest
{
    [MaxLength(255)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Status { get; set; }

    [MaxLength(50)]
    public string? Priority { get; set; }

    public DateTime? DueDate { get; set; }

    public int? AssignedTo { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }
}

