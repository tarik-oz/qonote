using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qonote.Core.Domain.Entities;

namespace Qonote.Infrastructure.Persistence.Configurations;

public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> builder)
    {
        builder.Property(us => us.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(us => us.Currency)
            .HasMaxLength(10);

        builder.Property(us => us.BillingPeriod)
            .HasMaxLength(20);

        builder.HasIndex(us => new { us.UserId, us.StartDate, us.EndDate });

        builder.HasOne(us => us.Plan)
            .WithMany()
            .HasForeignKey(us => us.PlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
