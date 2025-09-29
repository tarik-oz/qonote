using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Admin.UserSubscriptions.ListUserSubscriptions;

public sealed class ListUserSubscriptionsQueryHandler : IRequestHandler<ListUserSubscriptionsQuery, List<UserSubscriptionDto>>
{
    private readonly IReadRepository<UserSubscription, int> _reader;
    private readonly IReadRepository<SubscriptionPlan, int> _planReader;

    public ListUserSubscriptionsQueryHandler(
        IReadRepository<UserSubscription, int> reader,
        IReadRepository<SubscriptionPlan, int> planReader)
    {
        _reader = reader;
        _planReader = planReader;
    }

    public async Task<List<UserSubscriptionDto>> Handle(ListUserSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var items = await _reader.GetAllAsync(us => us.UserId == request.UserId, cancellationToken);

        var planIds = items.Select(i => i.PlanId).Distinct().ToList();
        var plans = planIds.Count > 0
            ? await _planReader.GetAllAsync(p => planIds.Contains(p.Id), cancellationToken)
            : new List<SubscriptionPlan>();
        var planCodeById = plans.ToDictionary(p => p.Id, p => p.PlanCode);

        return items
            .OrderByDescending(us => us.StartDate)
            .Select(us => new UserSubscriptionDto(
                us.Id,
                planCodeById.TryGetValue(us.PlanId, out var code) ? code : string.Empty,
                us.StartDate,
                us.EndDate,
                us.PriceAmount,
                us.Currency,
                us.BillingPeriod
            ))
            .ToList();
    }
}
