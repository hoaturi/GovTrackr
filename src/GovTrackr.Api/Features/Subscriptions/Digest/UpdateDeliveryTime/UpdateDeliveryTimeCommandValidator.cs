using FluentValidation;

namespace GovTrackr.Api.Features.Subscriptions.Digest.UpdateDeliveryTime;

public class UpdateDeliveryTimeCommandValidator : AbstractValidator<UpdateDeliveryTimeCommand>
{
    public UpdateDeliveryTimeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Dto.Time)
            .IsInEnum();

        RuleFor(x => x.Dto.Frequency)
            .IsInEnum();
    }
}