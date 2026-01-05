using FluentValidation;

namespace RespectLedger.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        When(x => !string.IsNullOrWhiteSpace(x.Nickname), () =>
        {
            RuleFor(x => x.Nickname)
                .MinimumLength(3).WithMessage("Nickname must be at least 3 characters")
                .MaximumLength(50).WithMessage("Nickname must not exceed 50 characters")
                .Matches("^[a-zA-Z0-9_]+$").WithMessage("Nickname can only contain letters, numbers, and underscores");
        });

        When(x => x.Bio != null, () =>
        {
            RuleFor(x => x.Bio)
                .MaximumLength(500).WithMessage("Bio must not exceed 500 characters");
        });
    }
}
