using FluentValidation;

namespace GovTrackr.Api.Features.Subscriptions.Digest.Subscribe;

public class SubscribeToDigestCommandValidator : AbstractValidator<SubscribeToDigestCommand>
{
    public SubscribeToDigestCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Time)
            .IsInEnum();

        RuleFor(x => x.Frequency)
            .IsInEnum();
    }
}