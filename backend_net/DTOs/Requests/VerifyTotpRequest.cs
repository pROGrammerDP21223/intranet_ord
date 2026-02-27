using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class VerifyTotpRequest
{
    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;
}

