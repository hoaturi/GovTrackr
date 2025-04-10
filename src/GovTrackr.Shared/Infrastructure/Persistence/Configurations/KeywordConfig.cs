using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Common;

namespace Shared.Infrastructure.Persistence.Configurations;

public class KeywordConfig : IEntityTypeConfiguration<Keyword>
{

    public void Configure(EntityTypeBuilder<Keyword> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired();

        builder.HasIndex(p => p.Name)
            .IsUnique();
    }
}