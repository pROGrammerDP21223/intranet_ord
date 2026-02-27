using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class ClientEmailService : BaseEntity
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    [MaxLength(50)]
    public string EmailServiceType { get; set; } = string.Empty; // "pop-id" or "g-suite-id"

    [Required]
    public int Quantity { get; set; }

    // Navigation property
    [ForeignKey("ClientId")]
    public Client? Client { get; set; }
}

