using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qonote.Core.Domain.Entities;

namespace Qonote.Infrastructure.Persistence.Configurations;

public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> builder)
    {
        builder.HasKey(us => us.Id);

        builder.Property(us => us.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(us => us.PriceAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(us => us.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(us => us.CancellationReason)
            .HasMaxLength(500);

        builder.Property(us => us.ExternalSubscriptionId)
            .HasMaxLength(255);

        builder.Property(us => us.ExternalCustomerId)
            .HasMaxLength(255);

        builder.Property(us => us.ExternalPriceId)
            .HasMaxLength(255);

        builder.Property(us => us.PaymentProvider)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(us => us.UsedNoteCount)
            .HasDefaultValue(0);

        // Relationships
        builder.HasOne(us => us.User)
            .WithMany(u => u.UserSubscriptions)
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(us => us.Plan)
            .WithMany(p => p.UserSubscriptions)
            .HasForeignKey(us => us.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(us => us.UserId);
        builder.HasIndex(us => new { us.UserId, us.Status });
        builder.HasIndex(us => us.ExternalSubscriptionId);
        builder.HasIndex(us => us.Status);
    }
}
