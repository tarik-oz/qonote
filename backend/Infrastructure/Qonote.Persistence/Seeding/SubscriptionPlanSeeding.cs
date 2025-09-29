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

        async Task EnsurePlanAsync(string code, string name, int maxNotes)
        {
            var existing = await db.SubscriptionPlans.FirstOrDefaultAsync(p => p.PlanCode == code, cancellationToken);
            if (existing is null)
            {
                db.SubscriptionPlans.Add(new SubscriptionPlan
                {
                    PlanCode = code,
                    Name = name,
                    MaxNoteCount = maxNotes
                });
                await db.SaveChangesAsync(cancellationToken);
            }
            else if (existing.Name != name || existing.MaxNoteCount != maxNotes)
            {
                existing.Name = name;
                existing.MaxNoteCount = maxNotes;
                db.SubscriptionPlans.Update(existing);
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        await EnsurePlanAsync("FREE", "Free", 2);
        await EnsurePlanAsync("STANDARD", "Standard", 10);
        await EnsurePlanAsync("PREMIUM", "Premium", 30);
    }
}
