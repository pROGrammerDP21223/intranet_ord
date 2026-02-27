using FluentValidation;
using backend_net.DTOs.Requests;

namespace backend_net.Validators;

public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MaximumLength(100).WithMessage("Role name must not exceed 100 characters")
            .Matches("^[a-zA-Z0-9\\s-]+$").WithMessage("Role name can only contain letters, numbers, spaces, and hyphens");

        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage("Description must not exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

