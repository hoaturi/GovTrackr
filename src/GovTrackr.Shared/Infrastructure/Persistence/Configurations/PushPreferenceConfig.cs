using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Notification;

namespace Shared.Infrastructure.Persistence.Configurations;

public class PushPreferenceConfig : IEntityTypeConfiguration<PushPreference>
{

    public void Configure(EntityTypeBuilder<PushPreference> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenId)
            .IsRequired();

        builder.HasOne(x => x.Token)
            .WithMany(x => x.Preferences)
            .HasForeignKey(x => x.TokenId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SubCategory)
            .WithMany()
            .HasForeignKey(x => x.SubCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}