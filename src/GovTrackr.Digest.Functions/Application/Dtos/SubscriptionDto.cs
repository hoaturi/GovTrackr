namespace GovTrackr.Digest.Functions.Application.Dtos;

public record SubscriptionDto(
    Guid Id,
    string Email,
    string UnsubscribeToken);