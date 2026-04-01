using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class Client : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string CustomerNo { get; set; } = string.Empty;

    [Required]
    public DateTime FormDate { get; set; }

    // Package Amount Fields
    [Column(TypeName = "DECIMAL(18,2)")]
    public decimal? AmountWithoutGst { get; set; }

    [Column(TypeName = "DECIMAL(5,2)")]
    public decimal? GstPercentage { get; set; } // GST percentage (e.g., 18.00 for 18%)

    [Column(TypeName = "DECIMAL(18,2)")]
    public decimal? GstAmount { get; set; } // Calculated: AmountWithoutGst * (GstPercentage / 100)

    [Column(TypeName = "DECIMAL(18,2)")]
    public decimal? TotalPackage { get; set; } // Calculated: AmountWithoutGst + GstAmount

    // Client Information
    [Required]
    [MaxLength(255)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string ContactPerson { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Designation { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? WhatsAppNumber { get; set; }

    [MaxLength(255)]
    public string? EnquiryEmail { get; set; } // Email for enquiries (if different from main email)

    public bool UseWhatsAppService { get; set; } = false; // Enable/disable WhatsApp notifications

    public bool WhatsAppSameAsMobile { get; set; } = true; // If true, use Phone as WhatsAppNumber

    public bool UseSameEmailForEnquiries { get; set; } = true; // If true, use Email for enquiries

    [MaxLength(255)]
    public string? DomainName { get; set; }

    [MaxLength(500)]
    public string? CompanyLogo { get; set; }

    [MaxLength(50)]
    public string? GstNo { get; set; }

    // Guidelines
    [Column(TypeName = "TEXT")]
    public string? SpecificGuidelines { get; set; }

    // Navigation properties
    public ICollection<ClientService>? ClientServices { get; set; }
    public ICollection<ClientEmailService>? ClientEmailServices { get; set; }
    public ClientSeoDetail? ClientSeoDetail { get; set; }
    public ClientAdwordsDetail? ClientAdwordsDetail { get; set; }
    public ICollection<Transaction>? Transactions { get; set; }
    public ICollection<ClientProduct>? ClientProducts { get; set; }
    public ICollection<UserClient>? UserClients { get; set; }
    public ICollection<SalesPersonClient>? SalesPersonClients { get; set; }
    public ICollection<SalesManagerClient>? SalesManagerClients { get; set; }
    public ICollection<OwnerClient>? OwnerClients { get; set; }

    // Created/Updated by tracking
    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    // Status tracking
    public int? CreatedByUserId { get; set; } // User ID who created the client

    [MaxLength(50)]
    public string Status { get; set; } = "Approved"; // Pending, Approved, AssignedToSomeoneElse

    [MaxLength(255)]
    public string? AssignedToSalesPersonName { get; set; } // Name of sales person if assigned

    public bool IsPremium { get; set; } = false; // Show in premium clients showcase on home page
}

