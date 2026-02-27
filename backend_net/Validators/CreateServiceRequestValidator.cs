using FluentValidation;
using backend_net.DTOs.Requests;

namespace backend_net.Validators;

public class CreateServiceRequestValidator : AbstractValidator<CreateServiceRequest>
{
    public CreateServiceRequestValidator()
    {
        RuleFor(x => x.ServiceType)
            .NotEmpty().WithMessage("Service type is required")
            .MaximumLength(100).WithMessage("Service type must not exceed 100 characters");

        RuleFor(x => x.ServiceName)
            .NotEmpty().WithMessage("Service name is required")
            .MaximumLength(255).WithMessage("Service name must not exceed 255 characters");

        RuleFor(x => x.Category)
            .MaximumLength(50).WithMessage("Category must not exceed 50 characters");
    }
}

