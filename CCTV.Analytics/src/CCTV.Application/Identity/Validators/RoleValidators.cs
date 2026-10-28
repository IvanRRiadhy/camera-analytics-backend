using CCTV.Application.Identity.DTOs;
using FluentValidation;

namespace CCTV.Application.Identity.Validators;

public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequestDto>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Role name is required.")
            .MaximumLength(100).WithMessage("Role name cannot exceed 100 characters.");
    }
}

public class UpdateRolePermissionsValidator : AbstractValidator<UpdateRolePermissionsDto>
{
    public UpdateRolePermissionsValidator()
    {
        RuleFor(x => x.Permissions)
            .NotNull().WithMessage("Permissions list cannot be null.");
    }
}
