using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class ClientProduct : BaseEntity
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    public int ProductId { get; set; }

    // Navigation properties
    [ForeignKey("ClientId")]
    public Client? Client { get; set; }

    [ForeignKey("ProductId")]
    public Product? Product { get; set; }
}

