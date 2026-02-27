using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class UpdateDashboardWidgetRequest
{
    [MaxLength(255)]
    public string? Title { get; set; }

    public int? PositionX { get; set; }

    public int? PositionY { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    [MaxLength(500)]
    public string? Config { get; set; }

    public bool? IsVisible { get; set; }
}

