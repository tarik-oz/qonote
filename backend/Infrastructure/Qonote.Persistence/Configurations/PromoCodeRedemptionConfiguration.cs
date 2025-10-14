using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Persistence.Configurations;

public class PromoCodeRedemptionConfiguration : IEntityTypeConfiguration<PromoCodeRedemption>
{
    public void Configure(EntityTypeBuilder<PromoCodeRedemption> builder)
    {
        builder.ToTable("PromoCodeRedemptions");

        builder.HasKey(r => r.Id);

        builder.HasIndex(r => new { r.PromoCodeId, r.UserId }).HasDatabaseName("IX_PromoCode_User");

        builder.Property(r => r.RedeemedAt).IsRequired();

        builder.HasOne(r => r.PromoCode)
            .WithMany(p => p.Redemptions)
            .HasForeignKey(r => r.PromoCodeId);

        builder.HasOne(r => r.UserSubscription)
            .WithMany()
            .HasForeignKey(r => r.UserSubscriptionId);
    }
}
