using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class CreateClientRequest
{
    [Required]
    [MaxLength(50)]
    public string CustomerNo { get; set; } = string.Empty;

    [Required]
    public DateTime FormDate { get; set; }

    // Package Amount Fields
    public decimal? AmountWithoutGst { get; set; }
    public decimal? GstPercentage { get; set; }
    public decimal? GstAmount { get; set; }
    public decimal? TotalPackage { get; set; }

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
    public string? EnquiryEmail { get; set; }

    public bool UseWhatsAppService { get; set; } = false;

    public bool WhatsAppSameAsMobile { get; set; } = true;

    public bool UseSameEmailForEnquiries { get; set; } = true;

    [MaxLength(255)]
    public string? DomainName { get; set; }

    [MaxLength(50)]
    public string? GstNo { get; set; }

    // Guidelines
    public string? SpecificGuidelines { get; set; }

    // Related Data
    public List<ClientServiceRequest>? Services { get; set; }
    public List<ClientEmailServiceRequest>? EmailServices { get; set; }
    public ClientSeoDetailRequest? SeoDetail { get; set; }
    public ClientAdwordsDetailRequest? AdwordsDetail { get; set; }
}

public class ClientServiceRequest
{
    [Required]
    public int ServiceId { get; set; }
}

public class ClientEmailServiceRequest
{
    [Required]
    [MaxLength(50)]
    public string EmailServiceType { get; set; } = string.Empty; // "pop-id" or "g-suite-id"

    [Required]
    public int Quantity { get; set; }
}

public class ClientSeoDetailRequest
{
    [MaxLength(50)]
    public string? KeywordRange { get; set; } // "upto-25", "25-50", "75-100"

    [MaxLength(255)]
    public string? Location { get; set; }

    public string? KeywordsList { get; set; }
}

public class ClientAdwordsDetailRequest
{
    [MaxLength(255)]
    public string? NumberOfKeywords { get; set; }

    [MaxLength(100)]
    public string? Period { get; set; } // "Monthly", "Quarterly"

    [MaxLength(255)]
    public string? Location { get; set; }

    public string? KeywordsList { get; set; }

    public string? SpecialGuidelines { get; set; }
}

