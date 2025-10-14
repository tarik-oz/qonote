using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Persistence.Configurations;

public class PromoCodeConfiguration : IEntityTypeConfiguration<PromoCode>
{
    public void Configure(EntityTypeBuilder<PromoCode> builder)
    {
        builder.ToTable("PromoCodes");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(p => p.Code).IsUnique();
        builder.HasIndex(p => new { p.IsActive, p.ExpiresAt });

        builder.Property(p => p.DurationMonths).IsRequired();
        builder.Property(p => p.RedemptionCount).IsRequired();

        builder.Property(p => p.RowVersion)
            .IsRowVersion();

        builder.HasMany(p => p.Redemptions)
            .WithOne(r => r.PromoCode!)
            .HasForeignKey(r => r.PromoCodeId);
    }
}
