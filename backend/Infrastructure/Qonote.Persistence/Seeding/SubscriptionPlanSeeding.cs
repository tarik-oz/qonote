using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qonote.Core.Domain.Entities;
using Qonote.Infrastructure.Persistence.Context;

namespace Qonote.Infrastructure.Persistence.Seeding;

public static class SubscriptionPlanSeeding
{
    public static async Task EnsureDefaultPlansAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
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

        await EnsurePlanAsync("FREE", "Free", "Perfect for getting started", 10, 0, 0, 0);
        await EnsurePlanAsync("PREMIUM", "Premium", "For power users", 100, 9.99m, 99.99m, 14);
        await EnsurePlanAsync("PREMIUM_PLUS", "Premium Plus", "Unlimited everything", -1, 19.99m, 199.99m, 14);
    }
}
