﻿using Microsoft.EntityFrameworkCore;
using Shared.Domain.Common;
using Shared.Domain.Digest;
using Shared.Domain.Notification;
using Shared.Domain.PresidentialAction;
using Shared.Domain.Subscription;

namespace Shared.Infrastructure.Persistence.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<PresidentialAction> PresidentialActions { get; set; }
    public DbSet<PresidentialActionTranslation> PresidentialActionTranslations { get; set; }
    public DbSet<DocumentSubCategory> DocumentSubCategories { get; set; }
    public DbSet<DocumentCategory> DocumentCategories { get; set; }
    public DbSet<Keyword> Keywords { get; set; }
    public DbSet<DigestSubscription> DigestSubscriptions { get; set; }
    public DbSet<Digest> Digests { get; set; }
    public DbSet<PushToken> PushTokens { get; set; }
    public DbSet<PushPreference> PushPreferences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var modifiedEntries = ChangeTracker.Entries()
            .Where(e => e is { Entity: BaseEntity, State: EntityState.Added or EntityState.Modified });

        foreach (var entry in modifiedEntries)
        {
            var entity = (BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added) entity.CreatedAt = DateTime.UtcNow;

            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}