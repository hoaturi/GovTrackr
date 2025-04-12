using FluentValidation;

namespace GovTrackr.Api.Features.Subscriptions.Digest.Subscribe;

public class SubscribeToDigestCommandValidator : AbstractValidator<SubscribeToDigestCommand>
{
    public SubscribeToDigestCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.");

        RuleFor(x => x.DeliveryTime)
            .IsInEnum()
            .WithMessage("Invalid delivery time.");
    }
}