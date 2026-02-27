using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class Transaction : BaseEntity
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    [MaxLength(50)]
    public string TransactionType { get; set; } = string.Empty; // Payment, Refund, Adjustment, etc.

    [Required]
    [MaxLength(100)]
    public string TransactionNumber { get; set; } = string.Empty; // Unique transaction number

    [Required]
    public DateTime TransactionDate { get; set; }

    [Required]
    [Column(TypeName = "DECIMAL(18,2)")]
    public decimal Amount { get; set; } // Positive for payments, negative for refunds

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? PaymentMethod { get; set; } // Cash, Bank Transfer, Credit Card, etc.

    [MaxLength(255)]
    public string? ReferenceNumber { get; set; } // Bank reference, cheque number, etc.

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation property
    [ForeignKey("ClientId")]
    public Client? Client { get; set; }

    // Created/Updated by tracking
    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }
}

// Transaction type constants
public static class TransactionTypes
{
    public const string Payment = "Payment";
    public const string Refund = "Refund";
    public const string Adjustment = "Adjustment";
    public const string Credit = "Credit";
    public const string Debit = "Debit";
}

