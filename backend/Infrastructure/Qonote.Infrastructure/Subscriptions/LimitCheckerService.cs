using Qonote.Core.Application.Abstractions.Subscriptions;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Enums;
using Qonote.Core.Domain.Entities;
using Qonote.Core.Application.Abstractions.Data;

namespace Qonote.Infrastructure.Infrastructure.Subscriptions;

public class LimitCheckerService : ILimitCheckerService
{
    private readonly IPlanResolver _planResolver;
    private readonly IReadRepository<UserSubscription, int> _userSubReader;
    private readonly IWriteRepository<UserSubscription, int> _userSubWriter;
    private readonly IReadRepository<SubscriptionPlan, int> _planReader;

    public LimitCheckerService(
        IPlanResolver planResolver,
        IReadRepository<UserSubscription, int> userSubReader,
        IWriteRepository<UserSubscription, int> userSubWriter,
        IReadRepository<SubscriptionPlan, int> planReader)
    {
        _planResolver = planResolver;
        _userSubReader = userSubReader;
        _userSubWriter = userSubWriter;
        _planReader = planReader;
    }

    public async Task EnsureAndConsumeNoteQuotaAsync(string userId, CancellationToken cancellationToken = default)
    {
        var plan = await _planResolver.GetEffectivePlanAsync(userId, cancellationToken);

        // Unlimited → nothing to consume
        if (plan.MaxNoteCount == int.MaxValue) return;

        var now = DateTime.UtcNow;

        // Load active or current subscription row
        var candidates = await _userSubReader.GetAllAsync(us => us.UserId == userId
            && us.StartDate <= now
            && (us.EndDate == null || us.EndDate > now)
            && (us.Status == SubscriptionStatus.Active || us.Status == SubscriptionStatus.Trialing || (us.Status == SubscriptionStatus.Cancelled && us.EndDate > now)), cancellationToken);
        var sub = candidates.OrderByDescending(us => us.StartDate).FirstOrDefault();

        if (sub is null)
        {
            // Development clean state: FREE sub should exist from registration; to be safe, create on-the-fly.
            var freePlans = await _planReader.GetAllAsync(p => p.PlanCode == "FREE", cancellationToken);
            var freePlan = freePlans.FirstOrDefault()
                ?? throw new InvalidOperationException("FREE plan not seeded.");
            (var ps, var pe) = Qonote.Core.Application.Common.Subscriptions.SubscriptionPeriodHelper.ComputeContainingPeriod(now, BillingInterval.Monthly, now);
            sub = new UserSubscription
            {
                UserId = userId,
                PlanId = freePlan.Id,
                StartDate = now,
                Status = SubscriptionStatus.Active,
                BillingInterval = BillingInterval.Monthly,
                AutoRenew = false,
                PaymentProvider = "Free",
                CurrentPeriodStart = ps,
                CurrentPeriodEnd = pe,
                UsedNoteCount = 0
            };
            await _userSubWriter.AddAsync(sub, cancellationToken);
        }

        // Ensure current period window is up-to-date; roll forward if needed
        var (start, end) = Qonote.Core.Application.Common.Subscriptions.SubscriptionPeriodHelper.ComputeContainingPeriod(
            sub.CurrentPeriodStart ?? sub.StartDate,
            sub.BillingInterval,
            now,
            sub.EndDate);
        if (sub.CurrentPeriodStart != start || sub.CurrentPeriodEnd != end)
        {
            sub.CurrentPeriodStart = start;
            sub.CurrentPeriodEnd = end;
            sub.UsedNoteCount = 0; // reset counter on period roll
            _userSubWriter.Update(sub);
        }

        if (sub.UsedNoteCount >= plan.MaxNoteCount)
        {
            var failures = new[]
            {
                new FluentValidation.Results.ValidationFailure(
                    "Limit.MaxNoteCount",
                    $"You reached the monthly note limit for your plan ({plan.PlanCode}): {sub.UsedNoteCount}/{plan.MaxNoteCount}.")
            };
            throw new ValidationException(failures);
        }

        // Consume one quota; defer SaveChanges to UnitOfWork so note + quota update commit atomically
        sub.UsedNoteCount += 1;
        _userSubWriter.Update(sub);
    }
}
