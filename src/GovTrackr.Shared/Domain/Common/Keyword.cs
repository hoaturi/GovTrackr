namespace Shared.Domain.Common;

public class Keyword : BaseEntity
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
}