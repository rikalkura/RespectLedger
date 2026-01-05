using FluentValidation;

namespace RespectLedger.Application.Features.Respects.Commands.GiveRespect;

public class GiveRespectCommandValidator : AbstractValidator<GiveRespectCommand>
{
    public GiveRespectCommandValidator()
    {
        RuleFor(x => x.SenderId)
            .NotEmpty().WithMessage("SenderId is required");

        RuleFor(x => x.ReceiverId)
            .NotEmpty().WithMessage("ReceiverId is required")
            .NotEqual(x => x.SenderId).WithMessage("Cannot give respect to yourself");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(280).WithMessage("Reason must not exceed 280 characters");

        RuleFor(x => x.Tag)
            .MaximumLength(50).WithMessage("Tag must not exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Tag));

        RuleFor(x => x.ImageUrl)
            .Must(BeValidUrl).WithMessage("ImageUrl must be a valid URL")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));
    }

    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out Uri? result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
