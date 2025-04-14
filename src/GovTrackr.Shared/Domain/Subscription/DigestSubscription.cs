using Shared.Domain.Common;

namespace Shared.Domain.Subscription;

public class DigestSubscription : BaseEntity
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string UnsubscribeToken { get; set; }
    public DateTime? LastSentAt { get; set; }
    public DigestSubscriptionStatus Status { get; set; } = DigestSubscriptionStatus.Active;
    public DateTime? StatusChangedAt { get; set; }
}

public enum DigestSubscriptionStatus
{
    Active,
    Inactive,
    Bounced,
    Complained,
    Blocked
}