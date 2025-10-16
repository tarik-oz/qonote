using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Abstractions.Subscriptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Users.GetMyPlan;

public sealed class GetMyPlanQueryHandler : IRequestHandler<GetMyPlanQuery, MyPlanDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPlanResolver _planResolver;
    private readonly IReadRepository<UserSubscription, int> _userSubReader;
    private readonly IReadRepository<SubscriptionPlan, int> _planReader;

    public GetMyPlanQueryHandler(
        ICurrentUserService currentUser,
        IPlanResolver planResolver,
        IReadRepository<UserSubscription, int> userSubReader,
        IReadRepository<SubscriptionPlan, int> planReader)
    {
        _currentUser = currentUser;
        _planResolver = planResolver;
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

        // Use the subscription's counter for current monthly usage
        var used = sub?.UsedNoteCount ?? 0;
        var remaining = Math.Max(0, effective.MaxNoteCount == int.MaxValue ? int.MaxValue : effective.MaxNoteCount - used);

        return new MyPlanDto(
            effective.PlanId,
            effective.PlanCode,
            planName,
            effective.MaxNoteCount,
            sub?.StartDate,
            sub?.EndDate,
            used,
            remaining
        );
    }
}
