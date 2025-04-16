using Shared.Domain.Common;

namespace Shared.Domain.Notification;

public class PushToken : BaseEntity
{
    public Guid Id { get; set; }
    public required string Token { get; set; }
    public ICollection<PushPreference> Preferences { get; set; } = [];
}