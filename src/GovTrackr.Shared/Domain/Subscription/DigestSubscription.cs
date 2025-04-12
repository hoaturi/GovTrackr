using Shared.Domain.Common;

namespace Shared.Domain.Subscription;

public class DigestSubscription : BaseEntity
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required DeliveryTime DeliveryTime { get; set; }
}

public enum DeliveryTime
{
    Morning = 1,
    Afternoon = 2,
    Evening = 3
}