using Shared.Domain.Common;

namespace Shared.Domain.PresidentialAction;

public class PresidentialActionTranslation : BaseEntity
{
    public Guid Id { get; set; }

    public Guid PresidentialActionId { get; set; }

    public required PresidentialAction PresidentialAction { get; set; }

    public required string Title { get; set; }

    public required string Content { get; set; }

    public required string Summary { get; set; }
}