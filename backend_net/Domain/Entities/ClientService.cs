using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class ClientService : BaseEntity
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    public int ServiceId { get; set; }

    // Navigation properties
    [ForeignKey("ClientId")]
    public Client? Client { get; set; }

    [ForeignKey("ServiceId")]
    public Service? Service { get; set; }
}

