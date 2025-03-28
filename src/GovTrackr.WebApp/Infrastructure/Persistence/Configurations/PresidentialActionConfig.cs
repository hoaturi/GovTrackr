using GovTrackr.Application.Domain.PresidentialAction;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovTrackr.Application.Infrastructure.Persistence.Configurations;

public class PresidentialActionConfig : IEntityTypeConfiguration<PresidentialAction>
{
    public void Configure(EntityTypeBuilder<PresidentialAction> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .IsRequired();

        builder.Property(p => p.Content)
            .IsRequired();

        builder.Property(p => p.SourceUrl)
            .IsRequired();

        builder.Property(p => p.PublishedAt)
            .IsRequired();

        builder.Property(p => p.TranslationStatus)
            .IsRequired();

        builder.HasOne(p => p.Classification)
            .WithMany()
            .HasForeignKey(p => p.ClassificationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.PublishedAt);
        builder.HasIndex(p => p.TranslationStatus);
    }
}