using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Digest;

namespace Shared.Infrastructure.Persistence.Configurations;

public class DigestConfig : IEntityTypeConfiguration<Digest>
{

    public void Configure(EntityTypeBuilder<Digest> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.EndDate)
            .IsRequired();

        builder.Property(x => x.Interval)
            .IsRequired();

        builder.HasIndex(x => x.StartDate);

        builder.HasIndex(x => x.Interval);

        builder.HasIndex(x => new { x.StartDate, x.EndDate, x.Interval })
            .IsUnique();
    }
}