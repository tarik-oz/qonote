using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qonote.Domain.Entities;

namespace Qonote.Infrastructure.Persistence.Configurations;

public class SectionUIStateConfiguration : IEntityTypeConfiguration<SectionUIState>
{
    public void Configure(EntityTypeBuilder<SectionUIState> builder)
    {
        // Composite Primary Key
        builder.HasKey(s => new { s.UserId, s.SectionId });

        // --- Relationships ---
        builder.HasOne(s => s.User)
            .WithMany(u => u.SectionUIStates)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Section)
            .WithMany(s => s.SectionUIStates)
            .HasForeignKey(s => s.SectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}