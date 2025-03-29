using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Common;

namespace Shared.Infrastructure.Persistence.Configurations;

public class DocumentCategoryConfig : IEntityTypeConfiguration<DocumentCategory>
{
    public void Configure(EntityTypeBuilder<DocumentCategory> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired();

        builder.Property(p => p.Slug)
            .IsRequired();

        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.HasIndex(p => p.Slug)
            .IsUnique();

        builder.HasData(
            new DocumentCategory
            {
                Id = (int)DocumentCategoryType.PresidentialAction,
                Name = "Presidential Action",
                Slug = "presidential-action"
            }
        );
    }
}