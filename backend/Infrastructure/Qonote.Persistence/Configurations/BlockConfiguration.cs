using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qonote.Core.Domain.Entities;

namespace Qonote.Infrastructure.Persistence.Configurations;

public class BlockConfiguration : IEntityTypeConfiguration<Block>
{
    public void Configure(EntityTypeBuilder<Block> builder)
    {
        builder.Property(b => b.Content).IsRequired();

        // Store enum as string
        builder.Property(b => b.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        // --- Relationships ---
        builder.HasOne(b => b.Section)
            .WithMany(s => s.Blocks)
            .HasForeignKey(b => b.SectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}