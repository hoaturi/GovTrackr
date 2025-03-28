using GovTrackr.Application.Domain.PresidentialAction;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovTrackr.Application.Infrastructure.Persistence.Configurations;

public class DocumentClassificationConfig : IEntityTypeConfiguration<DocumentClassification>
{
    public void Configure(EntityTypeBuilder<DocumentClassification> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired();

        builder.Property(p => p.Slug)
            .IsRequired();

        // Make Name and Slug unique within the same document type
        builder.HasIndex(p => new { p.Name, p.Type })
            .IsUnique();

        builder.HasIndex(p => new { p.Slug, p.Type })
            .IsUnique();

        builder.Property(p => p.Type);

        builder.HasIndex(p => p.Type);


        builder.HasData(
            new DocumentClassification
            {
                Id = 1,
                Name = "Executive Order",
                Slug = "executive-order",
                Type = DocumentType.PresidentialAction
            },
            new DocumentClassification
            {
                Id = 2,
                Name = "Presidential Memoranda",
                Slug = "presidential-memoranda",
                Type = DocumentType.PresidentialAction
            },
            new DocumentClassification
            {
                Id = 3,
                Name = "Proclamation",
                Slug = "proclamation",
                Type = DocumentType.PresidentialAction
            }
        );
    }
}