using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qonote.Core.Domain.Entities;
using Qonote.Infrastructure.Persistence.Context;

namespace Qonote.Infrastructure.Persistence.Seeding;

public static class SubscriptionSeeding
{
    public static async Task EnsureDefaultPlansAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync(cancellationToken);

        if (!await db.SubscriptionPlans.AnyAsync(p => p.PlanCode == "FREE" && !p.IsDeleted, cancellationToken))
        {
            db.SubscriptionPlans.Add(new SubscriptionPlan
            {
                PlanCode = "FREE",
                Name = "Free",
                MaxNoteCount = 2
            });
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
