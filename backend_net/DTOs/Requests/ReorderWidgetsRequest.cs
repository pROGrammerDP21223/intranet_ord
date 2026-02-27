using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class ReorderWidgetsRequest
{
    [Required]
    public List<int> WidgetIds { get; set; } = new();
}

