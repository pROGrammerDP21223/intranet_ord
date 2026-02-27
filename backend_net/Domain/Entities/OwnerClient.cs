using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class OwnerClient : BaseEntity
{
    [Required]
    public int OwnerId { get; set; } // User with Owner role

    [Required]
    public int ClientId { get; set; }

    // Navigation properties
    [ForeignKey("OwnerId")]
    public User? Owner { get; set; }

    [ForeignKey("ClientId")]
    public Client? Client { get; set; }
}
