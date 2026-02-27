using FluentValidation;
using backend_net.DTOs.Requests;

namespace backend_net.Validators;

public class AssignPermissionsRequestValidator : AbstractValidator<AssignPermissionsRequest>
{
    public AssignPermissionsRequestValidator()
    {
        RuleFor(x => x.PermissionIds)
            .NotNull().WithMessage("Permission IDs list is required")
            .Must(ids => ids != null && ids.Count > 0).WithMessage("At least one permission must be selected")
            .Must(ids => ids != null && ids.All(id => id > 0)).WithMessage("All permission IDs must be greater than 0")
            .Must(ids => ids != null && ids.Distinct().Count() == ids.Count).WithMessage("Permission IDs must be unique");
    }
}

