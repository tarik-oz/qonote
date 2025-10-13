using Microsoft.EntityFrameworkCore;
using Qonote.Core.Application.Abstractions.Subscriptions;
using Qonote.Infrastructure.Persistence.Context;
using Qonote.Core.Domain.Enums;

namespace Qonote.Infrastructure.Infrastructure.Subscriptions;

public class PlanResolver : IPlanResolver
{
    private readonly ApplicationDbContext _db;

    public PlanResolver(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<EffectivePlan> GetEffectivePlanAsync(string userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // Find active subscription - EndDate can be null for ongoing subscriptions
        var activeSub = await _db.UserSubscriptions
            .AsNoTracking()
            .Where(us => us.UserId == userId
                && !us.IsDeleted
                && us.StartDate <= now
                && (
                    us.Status == SubscriptionStatus.Active
                    || us.Status == SubscriptionStatus.Trialing
                    || (us.Status == SubscriptionStatus.Cancelled && us.EndDate != null && us.EndDate > now)
                   )
                && (us.EndDate == null || us.EndDate > now))
            .OrderByDescending(us => us.StartDate)
            .Select(us => new { us.PlanId, us.Plan!.PlanCode, us.Plan!.MaxNoteCount, us.Status, us.StartDate, us.EndDate })
            .FirstOrDefaultAsync(cancellationToken);

        if (activeSub is null)
        {
            // fallback to FREE plan
            var free = await _db.SubscriptionPlans
                .AsNoTracking()
                .Where(p => p.PlanCode == "FREE" && !p.IsDeleted)
                .Select(p => new { p.Id, p.PlanCode, p.MaxNoteCount })
                .FirstOrDefaultAsync(cancellationToken);

            if (free is null)
            {
                // If FREE not seeded yet, assume conservative defaults
                return new EffectivePlan(0, "FREE", 2);
            }

            // Interpret negative MaxNoteCount as unlimited
            var max = free.MaxNoteCount < 0 ? int.MaxValue : free.MaxNoteCount;
            return new EffectivePlan(free.Id, free.PlanCode, max);
        }
        else
        {
            // Interpret negative MaxNoteCount as unlimited
            var max = activeSub.MaxNoteCount < 0 ? int.MaxValue : activeSub.MaxNoteCount;
            return new EffectivePlan(activeSub.PlanId, activeSub.PlanCode, max);
        }
    }
}
