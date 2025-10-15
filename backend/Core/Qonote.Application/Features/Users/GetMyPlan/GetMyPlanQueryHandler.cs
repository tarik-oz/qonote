using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Queries;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Abstractions.Subscriptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Users.GetMyPlan;

public sealed class GetMyPlanQueryHandler : IRequestHandler<GetMyPlanQuery, MyPlanDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPlanResolver _planResolver;
    private readonly INoteQueries _noteQueries;
    private readonly IReadRepository<UserSubscription, int> _userSubReader;
    private readonly IReadRepository<SubscriptionPlan, int> _planReader;

    public GetMyPlanQueryHandler(
        ICurrentUserService currentUser,
        IPlanResolver planResolver,
        INoteQueries noteQueries,
        IReadRepository<UserSubscription, int> userSubReader,
        IReadRepository<SubscriptionPlan, int> planReader)
    {
        _currentUser = currentUser;
        _planResolver = planResolver;
        _noteQueries = noteQueries;
        _userSubReader = userSubReader;
        _planReader = planReader;
    }

    public async Task<MyPlanDto> Handle(GetMyPlanQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!;
        var now = DateTime.UtcNow;

        var effective = await _planResolver.GetEffectivePlanAsync(userId, cancellationToken);

        var subs = await _userSubReader.GetAllAsync(us => us.UserId == userId && us.StartDate <= now && (us.EndDate == null || us.EndDate > now), cancellationToken);
        var sub = subs.OrderByDescending(s => s.StartDate).FirstOrDefault();
        string planName;
        if (sub is not null)
        {
            var plan = await _planReader.GetByIdAsync(sub.PlanId, cancellationToken);
            planName = plan?.Name ?? effective.PlanCode;
        }
        else
        {
            planName = effective.PlanCode; // fallback
        }

        // Determine period window and count notes created in that window
        DateTime periodStart;
        DateTime periodEnd;
        if (sub is not null)
        {
            if (sub.CurrentPeriodStart.HasValue && sub.CurrentPeriodEnd.HasValue)
            {
                periodStart = sub.CurrentPeriodStart.Value;
                periodEnd = sub.CurrentPeriodEnd.Value;
            }
            else
            {
                var interval = sub.BillingInterval;
                (periodStart, periodEnd) = Common.Subscriptions.SubscriptionPeriodHelper.ComputeContainingPeriod(sub.StartDate, interval, now, sub.EndDate);
            }
        }
        else
        {
            // FREE users: anchor at user creation and use monthly window
            // We don't have ApplicationUser here via repository, so fall back to last 30 days aligned monthly window from now
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            periodStart = monthStart;
            periodEnd = monthStart.AddMonths(1);
        }

        var createdThisPeriod = await _noteQueries.CountNotesCreatedInPeriodAsync(userId, periodStart, periodEnd, cancellationToken);
        var remaining = Math.Max(0, effective.MaxNoteCount == int.MaxValue ? int.MaxValue : effective.MaxNoteCount - createdThisPeriod);

        return new MyPlanDto(
            effective.PlanId,
            effective.PlanCode,
            planName,
            effective.MaxNoteCount,
            sub?.StartDate,
            sub?.EndDate,
            createdThisPeriod,
            remaining
        );
    }
}
