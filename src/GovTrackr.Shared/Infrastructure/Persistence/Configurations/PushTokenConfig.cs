using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Notification;

namespace Shared.Infrastructure.Persistence.Configurations;

public class PushTokenConfig : IEntityTypeConfiguration<PushToken>
{

    public void Configure(EntityTypeBuilder<PushToken> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Token)
            .IsRequired();

        builder.HasMany(x => x.Preferences)
            .WithOne(x => x.Token)
            .HasForeignKey(x => x.TokenId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}