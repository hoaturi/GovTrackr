using Shared.Domain.Common;

namespace Shared.Domain.Digest;

public class Digest : BaseEntity
{
    public Guid Id { get; set; }
    public required string Content { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public required DigestInterval Interval { get; set; }
}

public enum DigestInterval
{
    Daily,
    Weekly,
    Monthly
}