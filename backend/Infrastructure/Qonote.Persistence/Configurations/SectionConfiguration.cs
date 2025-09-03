using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qonote.Domain.Entities;

namespace Qonote.Infrastructure.Persistence.Configurations;

public class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(300);

        // --- Relationships ---
        builder.HasOne(s => s.Note)
            .WithMany(n => n.Sections)
            .HasForeignKey(s => s.NoteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}