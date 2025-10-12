using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Features.Subscriptions._Shared;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Admin.UserSubscriptions.ListUserSubscriptions;

public sealed class ListUserSubscriptionsQueryHandler : IRequestHandler<ListUserSubscriptionsQuery, List<UserSubscriptionDto>>
{
    private readonly IReadRepository<UserSubscription, int> _reader;
    private readonly IReadRepository<SubscriptionPlan, int> _planReader;
    private readonly ILogger<ListUserSubscriptionsQueryHandler> _logger;

    public ListUserSubscriptionsQueryHandler(
        IReadRepository<UserSubscription, int> reader,
        IReadRepository<SubscriptionPlan, int> planReader,
        ILogger<ListUserSubscriptionsQueryHandler> logger)
    {
        _reader = reader;
        _planReader = planReader;
        _logger = logger;
    }

    public async Task<List<UserSubscriptionDto>> Handle(ListUserSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Admin ListUserSubscriptions started. userId={UserId}", request.UserId);
        var items = await _reader.GetAllAsync(us => us.UserId == request.UserId, cancellationToken);

        var planIds = items.Select(i => i.PlanId).Distinct().ToList();
        var plans = planIds.Count > 0
            ? await _planReader.GetAllAsync(p => planIds.Contains(p.Id), cancellationToken)
            : new List<SubscriptionPlan>();
        var planDict = plans.ToDictionary(p => p.Id);

        var result = items
            .OrderByDescending(us => us.StartDate)
            .Select(us => new UserSubscriptionDto
            {
                Id = us.Id,
                PlanName = planDict.TryGetValue(us.PlanId, out var plan) ? plan.Name : string.Empty,
                PlanCode = planDict.TryGetValue(us.PlanId, out var p) ? p.PlanCode : string.Empty,
                StartDate = us.StartDate,
                EndDate = us.EndDate,
                Status = us.Status,
                AutoRenew = us.AutoRenew,
                PriceAmount = us.PriceAmount,
                Currency = us.Currency,
                BillingInterval = us.BillingInterval,
                TrialEndDate = us.TrialEndDate,
                CancelAtPeriodEnd = us.CancelAtPeriodEnd,
                CurrentPeriodEnd = us.CurrentPeriodEnd
            })
            .ToList();
        _logger.LogDebug("Admin ListUserSubscriptions returning {Count} items for userId={UserId}", result.Count, request.UserId);
        return result;
    }
}
