using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class UpdateTransactionRequest
{
    [Required]
    [MaxLength(50)]
    public string TransactionType { get; set; } = string.Empty;

    [Required]
    public DateTime TransactionDate { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? PaymentMethod { get; set; }

    [MaxLength(255)]
    public string? ReferenceNumber { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

