using Shared.Domain.Common;

namespace Shared.Domain.Notification;

public class PushPreference : BaseEntity
{
    public Guid Id { get; set; }
    public Guid TokenId { get; set; }
    public PushToken Token { get; set; } = null!;
    public int SubCategoryId { get; set; }
    public DocumentSubCategory SubCategory { get; set; } = null!;
}