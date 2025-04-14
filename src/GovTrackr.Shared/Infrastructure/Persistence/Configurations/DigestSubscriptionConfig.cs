using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Subscription;

namespace Shared.Infrastructure.Persistence.Configurations;

public class DigestSubscriptionConfig : IEntityTypeConfiguration<DigestSubscription>
{

    public void Configure(EntityTypeBuilder<DigestSubscription> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.UnsubscribeToken)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(x => x.StatusChangedAt)
            .IsRequired(false);

        builder.Property(x => x.LastSentAt)
            .IsRequired(false);

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.HasIndex(x => x.UnsubscribeToken)
            .IsUnique();

        builder.HasIndex(x => x.Status)
            .IsUnique(false);
    }
}