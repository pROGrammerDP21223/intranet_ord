using FluentValidation;
using backend_net.DTOs.Requests;
using backend_net.Domain.Entities;

namespace backend_net.Validators;

public class UpdateTransactionRequestValidator : AbstractValidator<UpdateTransactionRequest>
{
    public UpdateTransactionRequestValidator()
    {
        RuleFor(x => x.TransactionType)
            .NotEmpty().WithMessage("Transaction type is required.")
            .MaximumLength(50).WithMessage("Transaction type must not exceed 50 characters.")
            .Must(BeValidTransactionType).WithMessage("Invalid transaction type. Valid types are: Payment, Refund, Adjustment, Credit, Debit.");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("Transaction date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Transaction date cannot be in the future.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.")
            .PrecisionScale(18, 2, false).WithMessage("Amount cannot have more than 2 decimal places.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.PaymentMethod)
            .MaximumLength(100).WithMessage("Payment method must not exceed 100 characters.");

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(255).WithMessage("Reference number must not exceed 255 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.");
    }

    private bool BeValidTransactionType(string transactionType)
    {
        var validTypes = new[]
        {
            TransactionTypes.Payment,
            TransactionTypes.Refund,
            TransactionTypes.Adjustment,
            TransactionTypes.Credit,
            TransactionTypes.Debit
        };
        return validTypes.Contains(transactionType, StringComparer.OrdinalIgnoreCase);
    }
}

