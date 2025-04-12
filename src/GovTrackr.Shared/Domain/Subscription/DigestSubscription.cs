using Shared.Domain.Common;

namespace Shared.Domain.Subscription;

public class DigestSubscription : BaseEntity
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required DeliveryTime DeliveryTime { get; set; }
    public required DeliveryFrequency DeliveryFrequency { get; set; }
    public required string UnsubscribeToken { get; set; }
    public DateTime? LastSentAt { get; set; }
}

public enum DeliveryTime
{
    Morning = 1,
    Afternoon = 2,
    Evening = 3
}

public enum DeliveryFrequency
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3
}