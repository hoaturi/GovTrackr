using Shared.Domain.Subscription;

namespace GovTrackr.Api.Features.Subscriptions.Digest.GetDigest;

public class GetDigestResponse
{
    public DeliveryTime DeliveryTime { get; set; }
    public DeliveryFrequency DeliveryFrequency { get; set; }
    public DateTime? LastSentAt { get; set; }
}