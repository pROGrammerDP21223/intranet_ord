using FluentValidation;
using backend_net.DTOs.Requests;

namespace backend_net.Validators;

public class CreateClientRequestValidator : AbstractValidator<CreateClientRequest>
{
    public CreateClientRequestValidator()
    {
        RuleFor(x => x.CustomerNo)
            .NotEmpty().WithMessage("Customer number is required")
            .MaximumLength(50).WithMessage("Customer number must not exceed 50 characters");

        RuleFor(x => x.FormDate)
            .NotEmpty().WithMessage("Form date is required");

        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required")
            .MaximumLength(255).WithMessage("Company name must not exceed 255 characters");

        RuleFor(x => x.ContactPerson)
            .NotEmpty().WithMessage("Contact person is required")
            .MaximumLength(255).WithMessage("Contact person must not exceed 255 characters");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.Email))
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters");

        RuleFor(x => x.GstNo)
            .MaximumLength(50).WithMessage("GST number must not exceed 50 characters");

        RuleFor(x => x.AmountWithoutGst)
            .GreaterThanOrEqualTo(0).WithMessage("Amount without GST must be greater than or equal to 0")
            .When(x => x.AmountWithoutGst.HasValue);

        RuleFor(x => x.GstPercentage)
            .InclusiveBetween(0, 100).WithMessage("GST percentage must be between 0 and 100")
            .When(x => x.GstPercentage.HasValue);
    }
}

