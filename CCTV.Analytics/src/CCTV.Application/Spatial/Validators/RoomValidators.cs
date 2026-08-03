using CCTV.Application.Spatial.DTOs;
using FluentValidation;

namespace CCTV.Application.Spatial.Validators;

public class CreateRoomRequestValidator : AbstractValidator<CreateRoomRequest>
{
    public CreateRoomRequestValidator()
    {
        RuleFor(x => x.FloorId)
            .NotEmpty().WithMessage("FloorId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Room Name is required.")
            .MaximumLength(150).WithMessage("Room Name must not exceed 150 characters.");

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Room Code must not exceed 50 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }
}

public class UpdateRoomRequestValidator : AbstractValidator<UpdateRoomRequest>
{
    public UpdateRoomRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Room Name is required.")
            .MaximumLength(150).WithMessage("Room Name must not exceed 150 characters.");

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Room Code must not exceed 50 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }
}
