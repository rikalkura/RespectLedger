using FluentValidation;

namespace RespectLedger.Application.Features.Users.Commands.UploadAvatar;

public class UploadAvatarCommandValidator : AbstractValidator<UploadAvatarCommand>
{
    public UploadAvatarCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("ImageUrl is required")
            .Must(BeValidUrl).WithMessage("ImageUrl must be a valid URL");
    }

    private static bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri? result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
