using FluentValidation;

namespace GovTrackr.Api.Features.Subscriptions.Digest.UnsubscribeDigest;

public class UnsubscribeDigestCommandValidator : AbstractValidator<UnsubscribeDigestCommand>
{
    public UnsubscribeDigestCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Token is required.");
    }
}