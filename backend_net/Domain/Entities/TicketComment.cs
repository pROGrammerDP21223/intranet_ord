using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class TicketComment : BaseEntity
{
    [Required]
    public int TicketId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [Column(TypeName = "TEXT")]
    public string Comment { get; set; } = string.Empty;

    public bool IsInternal { get; set; } = false; // If true, only staff/admin can see this comment

    // Navigation properties
    [ForeignKey("TicketId")]
    public Ticket? Ticket { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; }
}

