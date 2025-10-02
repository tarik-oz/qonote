using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qonote.Core.Domain.Entities;

namespace Qonote.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(p => p.Amount)
            .HasPrecision(9, 2)
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(p => p.ExternalPaymentId)
            .HasMaxLength(255);

        builder.Property(p => p.ExternalChargeId)
            .HasMaxLength(255);

        builder.Property(p => p.ExternalInvoiceId)
            .HasMaxLength(255);

        builder.Property(p => p.PaymentProvider)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.FailureCode)
            .HasMaxLength(100);

        builder.Property(p => p.FailureMessage)
            .HasMaxLength(500);

        builder.Property(p => p.RefundedAmount)
            .HasPrecision(18, 2);

        builder.Property(p => p.RefundReason)
            .HasMaxLength(500);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.InvoiceUrl)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(p => p.User)
            .WithMany(u => u.Payments)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.UserSubscription)
            .WithMany(s => s.Payments)
            .HasForeignKey(p => p.UserSubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.UserSubscriptionId);
        builder.HasIndex(p => p.ExternalPaymentId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.CreatedAt);
    }
}

