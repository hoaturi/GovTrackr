using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Common;

namespace Shared.Infrastructure.Persistence.Configurations;

public class DocumentSubCategoryConfig : IEntityTypeConfiguration<DocumentSubCategory>
{
    public void Configure(EntityTypeBuilder<DocumentSubCategory> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired();

        builder.Property(p => p.Slug)
            .IsRequired();

        // Make Name and Slug unique within the same document type
        builder.HasIndex(p => new { p.Name, p.CategoryId })
            .IsUnique();

        builder.HasIndex(p => new { p.Slug, p.CategoryId })
            .IsUnique();

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new DocumentSubCategory
            {
                Id = (int)DocumentSubCategoryType.ExecutiveOrder,
                Name = "Executive Order",
                Slug = "executive-order",
                CategoryId = (int)DocumentCategoryType.PresidentialAction
            },
            new DocumentSubCategory
            {
                Id = (int)DocumentSubCategoryType.Memoranda,
                Name = "Memoranda",
                Slug = "memoranda",
                CategoryId = (int)DocumentCategoryType.PresidentialAction
            },
            new DocumentSubCategory
            {
                Id = (int)DocumentSubCategoryType.Proclamation,
                Name = "Proclamation",
                Slug = "proclamation",
                CategoryId = (int)DocumentCategoryType.PresidentialAction
            },
            new DocumentSubCategory
            {
                Id = (int)DocumentSubCategoryType.Nomination,
                Name = "Nomination",
                Slug = "nomination",
                CategoryId = (int)DocumentCategoryType.PresidentialAction
            }
        );
    }
}