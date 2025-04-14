using FluentValidation;

namespace GovTrackr.Api.Features.Subscriptions.Digest.SubscribeToDigest;

public class SubscribeToDigestCommandValidator : AbstractValidator<SubscribeToDigestCommand>
{
    public SubscribeToDigestCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}