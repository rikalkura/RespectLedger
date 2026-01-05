using FluentValidation;

namespace RespectLedger.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Nickname)
            .NotEmpty().WithMessage("Nickname is required")
            .MinimumLength(3).WithMessage("Nickname must be at least 3 characters")
            .MaximumLength(50).WithMessage("Nickname must not exceed 50 characters")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Nickname can only contain letters, numbers, and underscores");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters");
    }
}
