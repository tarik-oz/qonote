using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qonote.Core.Domain.Entities;

namespace Qonote.Infrastructure.Persistence.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PlanCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.MonthlyPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.YearlyPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(p => p.ExternalProductId)
            .HasMaxLength(255);

        builder.Property(p => p.ExternalPriceIdMonthly)
            .HasMaxLength(255);

        builder.Property(p => p.ExternalPriceIdYearly)
            .HasMaxLength(255);

        // Indexes
        builder.HasIndex(p => p.PlanCode).IsUnique();
        builder.HasIndex(p => p.IsActive);
    }
}
