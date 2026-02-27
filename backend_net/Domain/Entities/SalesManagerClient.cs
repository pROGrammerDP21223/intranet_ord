using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class SalesManagerClient : BaseEntity
{
    [Required]
    public int SalesManagerId { get; set; } // User with Sales Manager role

    [Required]
    public int ClientId { get; set; }

    // Navigation properties
    [ForeignKey("SalesManagerId")]
    public User? SalesManager { get; set; }

    [ForeignKey("ClientId")]
    public Client? Client { get; set; }
}
