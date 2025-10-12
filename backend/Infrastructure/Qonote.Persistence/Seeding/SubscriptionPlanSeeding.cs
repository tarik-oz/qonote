using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qonote.Core.Domain.Entities;
using Qonote.Infrastructure.Persistence.Context;
using Microsoft.Extensions.Configuration;

namespace Qonote.Infrastructure.Persistence.Seeding;

public static class SubscriptionPlanSeeding
{
    public static async Task EnsureDefaultPlansAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var configuration = scope.ServiceProvider.GetService<IConfiguration>();
        var updateCoreFields = true;
        if (configuration is not null)
        {
            var val = configuration.GetSection("Seeding")["UpdatePlanCoreFields"];
            if (!string.IsNullOrWhiteSpace(val) && bool.TryParse(val, out var parsed))
            {
                updateCoreFields = parsed;
            }
        }
        await db.Database.MigrateAsync(cancellationToken);

        async Task EnsurePlanAsync(
            string code,
            string name,
            string description,
            int maxNotes,
            decimal monthlyPrice,
            decimal yearlyPrice,
            int trialDays = 0)
        {
            var existing = await db.SubscriptionPlans.FirstOrDefaultAsync(p => p.PlanCode == code, cancellationToken);
            if (existing is null)
            {
                db.SubscriptionPlans.Add(new SubscriptionPlan
                {
                    PlanCode = code,
                    Name = name,
                    Description = description,
                    MaxNoteCount = maxNotes,
                    MonthlyPrice = monthlyPrice,
                    YearlyPrice = yearlyPrice,
                    Currency = "USD",
                    TrialDays = trialDays,
                    IsActive = true
                });
                await db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                if (updateCoreFields)
                {
                    var updated = false;
                    if (existing.Name != name) { existing.Name = name; updated = true; }
                    if (existing.Description != description) { existing.Description = description; updated = true; }
                    if (existing.MaxNoteCount != maxNotes) { existing.MaxNoteCount = maxNotes; updated = true; }
                    if (existing.MonthlyPrice != monthlyPrice) { existing.MonthlyPrice = monthlyPrice; updated = true; }
                    if (existing.YearlyPrice != yearlyPrice) { existing.YearlyPrice = yearlyPrice; updated = true; }
                    if (existing.TrialDays != trialDays) { existing.TrialDays = trialDays; updated = true; }

                    if (updated)
                    {
                        db.SubscriptionPlans.Update(existing);
                        await db.SaveChangesAsync(cancellationToken);
                    }
                }
            }
        }

        await EnsurePlanAsync("FREE", "Free", "Perfect for getting started", 10, 0, 0, 0);
        await EnsurePlanAsync("PREMIUM", "Premium", "For power users", 100, 9.99m, 99.99m, 14);
        await EnsurePlanAsync("PREMIUM_PLUS", "Premium Plus", "Unlimited everything", -1, 19.99m, 199.99m, 14);

        // Optionally update external IDs for Lemon Squeezy from configuration
        // Expected structure:
        //  "LemonSqueezy": {
        //    "PlanMappings": [
        //      { "PlanCode": "PREMIUM", "ProductId": "123", "MonthlyVariantId": "456", "YearlyVariantId": "789" }
        //    ]
        //  }
        if (configuration is not null)
        {
            var mappingSection = configuration.GetSection("LemonSqueezy:PlanMappings");
            var children = mappingSection.GetChildren();
            if (children.Any())
            {
                var plans = await db.SubscriptionPlans.ToListAsync(cancellationToken);
                var changed = false;
                foreach (var child in children)
                {
                    var planCode = child["PlanCode"] ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(planCode)) continue;
                    var productId = child["ProductId"];
                    var monthlyVariantId = child["MonthlyVariantId"];
                    var yearlyVariantId = child["YearlyVariantId"];

                    var plan = plans.FirstOrDefault(p => p.PlanCode == planCode);
                    if (plan is null) continue;
                    if (!string.IsNullOrWhiteSpace(productId) && plan.ExternalProductId != productId)
                    { plan.ExternalProductId = productId; changed = true; }
                    if (!string.IsNullOrWhiteSpace(monthlyVariantId) && plan.ExternalPriceIdMonthly != monthlyVariantId)
                    { plan.ExternalPriceIdMonthly = monthlyVariantId; changed = true; }
                    if (!string.IsNullOrWhiteSpace(yearlyVariantId) && plan.ExternalPriceIdYearly != yearlyVariantId)
                    { plan.ExternalPriceIdYearly = yearlyVariantId; changed = true; }
                }
                if (changed)
                {
                    await db.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }

    // Configuration helper contract (kept for documentation; not used at runtime parsing)
    private sealed class PlanMapping
    {
        public string PlanCode { get; set; } = string.Empty;
        public string? ProductId { get; set; }
        public string? MonthlyVariantId { get; set; }
        public string? YearlyVariantId { get; set; }
    }
}
