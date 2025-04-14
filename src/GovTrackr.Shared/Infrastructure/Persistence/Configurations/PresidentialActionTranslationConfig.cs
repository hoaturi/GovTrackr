using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.PresidentialAction;

namespace Shared.Infrastructure.Persistence.Configurations;

public class PresidentialActionTranslationConfig : IEntityTypeConfiguration<PresidentialActionTranslation>
{
    public void Configure(EntityTypeBuilder<PresidentialActionTranslation> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired();

        builder.Property(t => t.Content)
            .IsRequired();

        builder.Property(t => t.Summary)
            .IsRequired();

        builder.Property(t => t.PresidentialActionId)
            .IsRequired();

        builder.HasIndex(t => t.PresidentialActionId)
            .IsUnique();

        builder.HasOne(t => t.PresidentialAction)
            .WithOne(p => p.Translation)
            .HasForeignKey<PresidentialActionTranslation>(t => t.PresidentialActionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Keywords)
            .WithMany(k => k.PresidentialActionTranslations)
            .UsingEntity(j => j.ToTable("PresidentialActionTranslationKeywords"));
    }
}