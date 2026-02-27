using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class SalesPersonClient : BaseEntity
{
    [Required]
    public int SalesPersonId { get; set; } // User with Sales Person role

    [Required]
    public int ClientId { get; set; }

    // Navigation properties
    [ForeignKey("SalesPersonId")]
    public User? SalesPerson { get; set; }

    [ForeignKey("ClientId")]
    public Client? Client { get; set; }
}

