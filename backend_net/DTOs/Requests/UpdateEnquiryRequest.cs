using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class UpdateEnquiryRequest
{
    [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    public string? Status { get; set; }

    [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}

