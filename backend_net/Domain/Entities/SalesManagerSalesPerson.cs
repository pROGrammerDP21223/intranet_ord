using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class SalesManagerSalesPerson : BaseEntity
{
    [Required]
    public int SalesManagerId { get; set; } // User with Sales Manager role

    [Required]
    public int SalesPersonId { get; set; } // User with Sales Person role

    // Navigation properties
    [ForeignKey("SalesManagerId")]
    public User? SalesManager { get; set; }

    [ForeignKey("SalesPersonId")]
    public User? SalesPerson { get; set; }
}

