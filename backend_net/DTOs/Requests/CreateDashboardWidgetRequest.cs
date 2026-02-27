using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class CreateDashboardWidgetRequest
{
    [Required]
    [MaxLength(100)]
    public string WidgetType { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public int PositionX { get; set; } = 0;

    public int PositionY { get; set; } = 0;

    public int Width { get; set; } = 4;

    public int Height { get; set; } = 3;

    [MaxLength(500)]
    public string? Config { get; set; }

    public bool IsVisible { get; set; } = true;
}

