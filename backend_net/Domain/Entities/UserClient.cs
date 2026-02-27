using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class UserClient : BaseEntity
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int ClientId { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public User? User { get; set; }

    [ForeignKey("ClientId")]
    public Client? Client { get; set; }
}

