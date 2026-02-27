using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class UpdateEmailTemplateRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(255)]
    public string? Subject { get; set; }

    public string? Body { get; set; }

    [MaxLength(50)]
    public string? TemplateType { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    [MaxLength(500)]
    public string? Variables { get; set; }
}

