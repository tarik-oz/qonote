using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qonote.Core.Domain.Entities;

namespace Qonote.Infrastructure.Persistence.Configurations;

public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        // --- Properties ---
        builder.Property(n => n.CustomTitle)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(n => n.YoutubeUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(n => n.VideoTitle)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(n => n.ThumbnailUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(n => n.ChannelName)
            .IsRequired()
            .HasMaxLength(150);

        // Normalized title for case-insensitive uniqueness per user
        // Uses a computed stored column in PostgreSQL: lower("CustomTitle")
        builder.Property<string>("CustomTitleNormalized")
            .HasComputedColumnSql("lower(btrim(\"CustomTitle\"))", stored: true);

        // Indexes for sidebar queries
        builder.HasIndex(n => new { n.UserId, n.IsDeleted, n.UpdatedAt });
        builder.HasIndex(n => new { n.NoteGroupId, n.IsDeleted, n.UpdatedAt });

        // Unique index: enforce that (UserId, lower(CustomTitle)) is unique for non-deleted notes
        // This allows duplicates when previous note is soft-deleted.
        builder.HasIndex("UserId", "CustomTitleNormalized")
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // --- Relationships ---
        builder.HasOne(n => n.User)
            .WithMany(u => u.Notes)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.NoteGroup)
            .WithMany(ng => ng.Notes)
            .HasForeignKey(n => n.NoteGroupId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}