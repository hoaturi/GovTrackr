using FluentValidation;

namespace GovTrackr.Api.Features.Notifications.RegisterPushPreferences;

public class RegisterPushPreferencesCommandValidator : AbstractValidator<RegisterPushPreferencesCommand>
{
    public RegisterPushPreferencesCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty();

        RuleFor(x => x.DocumentType)
            .IsInEnum();
    }
}