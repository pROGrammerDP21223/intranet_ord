using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class DashboardWidget : BaseEntity
{
    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string WidgetType { get; set; } = string.Empty; // revenue_chart, client_count, etc.

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public int PositionX { get; set; } = 0;

    public int PositionY { get; set; } = 0;

    public int Width { get; set; } = 4; // Grid columns (1-12)

    public int Height { get; set; } = 3; // Grid rows

    [MaxLength(500)]
    public string? Config { get; set; } // JSON string for widget-specific configuration

    public bool IsVisible { get; set; } = true;

    // Navigation property
    [ForeignKey("UserId")]
    public User? User { get; set; }
}

