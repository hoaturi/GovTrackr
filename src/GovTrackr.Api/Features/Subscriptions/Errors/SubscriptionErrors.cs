using GovTrackr.Api.Common.Errors;

namespace GovTrackr.Api.Features.Subscriptions.Errors;

public static class SubscriptionErrors
{
    public static readonly ConflictError EmailAlreadySubscribed =
        new("Email already subscribed.", ErrorCodes.EmailAlreadySubscribed);
}