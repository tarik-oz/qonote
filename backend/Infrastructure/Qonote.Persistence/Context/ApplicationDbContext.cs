using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Qonote.Core.Domain.Entities;
using Qonote.Core.Domain.Identity;
using Qonote.Core.Domain.Common;

namespace Qonote.Infrastructure.Persistence.Context;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Custom Domain Entities
    public DbSet<NoteGroup> NoteGroups { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<Block> Blocks { get; set; }
    public DbSet<SectionUIState> SectionUIStates { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        ApplyAuditInfo();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInfo()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries().ToList())
        {
            if (entry.Entity is IEntityBase<int> || entry.Entity is IEntityBase<Guid> || entry.Entity is IEntityBase<string>)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.CurrentValues[nameof(EntityBase<int>.CreatedAt)] = now;
                    entry.CurrentValues[nameof(EntityBase<int>.UpdatedAt)] = null;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.CurrentValues[nameof(EntityBase<int>.UpdatedAt)] = now;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    // Switch to soft delete if entity supports it
                    if (entry.Properties.Any(p => p.Metadata.Name == nameof(EntityBase<int>.IsDeleted)))
                    {
                        entry.State = EntityState.Modified;
                        entry.CurrentValues[nameof(EntityBase<int>.IsDeleted)] = true;
                        if (entry.Properties.Any(p => p.Metadata.Name == nameof(EntityBase<int>.DeletedAt)))
                        {
                            entry.CurrentValues[nameof(EntityBase<int>.DeletedAt)] = now;
                        }
                        entry.CurrentValues[nameof(EntityBase<int>.UpdatedAt)] = now;
                    }
                }
            }
        }

        // Parent bump rules
        // If Section changed -> bump Note.UpdatedAt
        var sectionChanges = ChangeTracker.Entries<Section>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .Select(e => e.Entity.NoteId)
            .Distinct()
            .ToList();
        if (sectionChanges.Count > 0)
        {
            var notes = Notes.Where(n => sectionChanges.Contains(n.Id)).ToList();
            foreach (var n in notes)
            {
                n.UpdatedAt = now;
            }
        }

        // If Block changed -> bump Section and Note
        var blockChanges = ChangeTracker.Entries<Block>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .Select(e => e.Entity.SectionId)
            .Distinct()
            .ToList();
        if (blockChanges.Count > 0)
        {
            var sections = Sections.Where(s => blockChanges.Contains(s.Id)).ToList();
            foreach (var s in sections)
            {
                s.UpdatedAt = now;
            }
            var noteIds = sections.Select(s => s.NoteId).Distinct().ToList();
            var notes2 = Notes.Where(n => noteIds.Contains(n.Id)).ToList();
            foreach (var n in notes2)
            {
                n.UpdatedAt = now;
            }
        }

        // If Note changed and has a group, bump group
        var noteChanges = ChangeTracker.Entries<Note>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .Select(e => e.Entity.NoteGroupId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        if (noteChanges.Count > 0)
        {
            var groups = NoteGroups.Where(g => noteChanges.Contains(g.Id)).ToList();
            foreach (var g in groups)
            {
                g.UpdatedAt = now;
            }
        }
    }
}
