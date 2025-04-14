using Shared.Domain.Subscription;

namespace GovTrackr.Api.Features.Subscriptions.Digest.GetDigest;

public class GetDigestResponse
{
    public DateTime? LastSentAt { get; set; }
}