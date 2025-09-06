using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qonote.Core.Domain.Entities;

namespace Qonote.Infrastructure.Persistence.Configurations;

public class NoteGroupConfiguration : IEntityTypeConfiguration<NoteGroup>
{
    public void Configure(EntityTypeBuilder<NoteGroup> builder)
    {
        builder.Property(ng => ng.Name)
            .IsRequired()
            .HasMaxLength(100);

        // --- Relationships ---
        builder.HasOne(ng => ng.User)
            .WithMany(u => u.NoteGroups)
            .HasForeignKey(ng => ng.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}