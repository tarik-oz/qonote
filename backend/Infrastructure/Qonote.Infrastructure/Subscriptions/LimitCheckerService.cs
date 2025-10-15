using Microsoft.EntityFrameworkCore;
using Qonote.Core.Application.Abstractions.Subscriptions;
using Qonote.Core.Application.Exceptions;
using Qonote.Infrastructure.Persistence.Context;
using Qonote.Core.Application.Abstractions.Queries;
using Qonote.Core.Domain.Enums;
using Qonote.Core.Domain.Entities;

namespace Qonote.Infrastructure.Infrastructure.Subscriptions;

public class LimitCheckerService : ILimitCheckerService
{
    private readonly ApplicationDbContext _db;
    private readonly IPlanResolver _planResolver;
    private readonly INoteQueries _noteQueries;

    public LimitCheckerService(ApplicationDbContext db, IPlanResolver planResolver, INoteQueries noteQueries)
    {
        _db = db;
        _planResolver = planResolver;
        _noteQueries = noteQueries;
    }

    public async Task EnsureUserCanCreateNoteAsync(string userId, CancellationToken cancellationToken = default)
    {
        var plan = await _planResolver.GetEffectivePlanAsync(userId, cancellationToken);

        // If unlimited, allow
        if (plan.MaxNoteCount == int.MaxValue)
        {
            return;
        }

        var now = DateTime.UtcNow;

        // Find the active subscription to determine current period window
        var sub = await _db.Set<UserSubscription>()
            .AsNoTracking()
            .Where(us => us.UserId == userId
                && !us.IsDeleted
                && us.StartDate <= now
                && (us.EndDate == null || us.EndDate > now)
                && (us.Status == SubscriptionStatus.Active || us.Status == SubscriptionStatus.Trialing || (us.Status == SubscriptionStatus.Cancelled && us.EndDate > now)))
            .OrderByDescending(us => us.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        DateTime periodStart;
        DateTime periodEnd;

        if (sub is not null)
        {
            // Prefer explicit current period if present; otherwise compute from StartDate and BillingInterval
            if (sub.CurrentPeriodStart.HasValue && sub.CurrentPeriodEnd.HasValue)
            {
                periodStart = sub.CurrentPeriodStart.Value;
                periodEnd = sub.CurrentPeriodEnd.Value;
            }
            else
            {
                var interval = sub.BillingInterval;
                (periodStart, periodEnd) = Qonote.Core.Application.Common.Subscriptions.SubscriptionPeriodHelper.ComputeContainingPeriod(sub.StartDate, interval, now, sub.EndDate);
            }
        }
        else
        {
            // No paid/trial subscription: treat as FREE. Create a synthetic month window anchored at account creation.
            var user = await _db.Users.AsNoTracking().Where(u => u.Id == userId).Select(u => new { u.CreatedAt }).FirstOrDefaultAsync(cancellationToken);
            var anchor = user?.CreatedAt ?? now; // fallback to now if missing
            (periodStart, periodEnd) = Qonote.Core.Application.Common.Subscriptions.SubscriptionPeriodHelper.ComputeContainingPeriod(anchor, BillingInterval.Monthly, now, null);
        }

        var createdThisPeriod = await _noteQueries.CountNotesCreatedInPeriodAsync(userId, periodStart, periodEnd, cancellationToken);

        if (createdThisPeriod >= plan.MaxNoteCount)
        {
            var failures = new[]
            {
                new FluentValidation.Results.ValidationFailure(
                    "Limit.MaxNoteCount",
                    $"You reached the monthly note limit for your plan ({plan.PlanCode}): {createdThisPeriod}/{plan.MaxNoteCount}.")
            };
            throw new ValidationException(failures);
        }
    }
}
