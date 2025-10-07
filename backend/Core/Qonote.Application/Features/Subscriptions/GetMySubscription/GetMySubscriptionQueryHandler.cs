using MediatR;
using Microsoft.EntityFrameworkCore;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Features.Subscriptions._Shared;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Subscriptions.GetMySubscription;

public sealed class GetMySubscriptionQueryHandler : IRequestHandler<GetMySubscriptionQuery, UserSubscriptionDto?>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IReadRepository<UserSubscription, int> _reader;
    private readonly IReadRepository<SubscriptionPlan, int> _plans;

    public GetMySubscriptionQueryHandler(ICurrentUserService currentUser, IReadRepository<UserSubscription, int> reader, IReadRepository<SubscriptionPlan, int> plans)
    {
        _currentUser = currentUser;
        _reader = reader;
        _plans = plans;
    }

    public async Task<UserSubscriptionDto?> Handle(GetMySubscriptionQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var items = await _reader.GetAllAsync(us => us.UserId == userId, cancellationToken);
        var active = items
            .Where(us => us.Status == Core.Domain.Enums.SubscriptionStatus.Active || us.Status == Core.Domain.Enums.SubscriptionStatus.Trialing || (us.Status == Core.Domain.Enums.SubscriptionStatus.Cancelled && (us.EndDate ?? us.CurrentPeriodEnd) > DateTime.UtcNow))
            .OrderByDescending(us => us.StartDate)
            .FirstOrDefault();

        if (active is null) return null;

        var plan = await _plans.GetByIdAsync(active.PlanId, cancellationToken);

        return new UserSubscriptionDto
        {
            Id = active.Id,
            PlanName = plan?.Name ?? string.Empty,
            PlanCode = plan?.PlanCode ?? string.Empty,
            StartDate = active.StartDate,
            EndDate = active.EndDate,
            Status = active.Status,
            AutoRenew = active.AutoRenew,
            PriceAmount = active.PriceAmount,
            Currency = active.Currency,
            BillingInterval = active.BillingInterval,
            TrialEndDate = active.TrialEndDate,
            CancelAtPeriodEnd = active.CancelAtPeriodEnd,
            CurrentPeriodEnd = active.CurrentPeriodEnd
        };
    }
}


