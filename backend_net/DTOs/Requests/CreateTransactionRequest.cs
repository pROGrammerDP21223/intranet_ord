using System.ComponentModel.DataAnnotations;

namespace backend_net.DTOs.Requests;

public class CreateTransactionRequest
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    [MaxLength(50)]
    public string TransactionType { get; set; } = string.Empty; // Payment, Refund, Adjustment, etc.

    [Required]
    public DateTime TransactionDate { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? PaymentMethod { get; set; } // Cash, Bank Transfer, Credit Card, etc.

    [MaxLength(255)]
    public string? ReferenceNumber { get; set; } // Bank reference, cheque number, etc.

    [MaxLength(500)]
    public string? Notes { get; set; }
}

