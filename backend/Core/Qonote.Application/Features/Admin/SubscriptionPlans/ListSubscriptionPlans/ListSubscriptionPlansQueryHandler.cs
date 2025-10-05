using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Features.Subscriptions._Shared;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.ListSubscriptionPlans;

public sealed class ListSubscriptionPlansQueryHandler : IRequestHandler<ListSubscriptionPlansQuery, List<SubscriptionPlanDto>>
{
    private readonly IReadRepository<SubscriptionPlan, int> _reader;
    public ListSubscriptionPlansQueryHandler(IReadRepository<SubscriptionPlan, int> reader) => _reader = reader;

    public async Task<List<SubscriptionPlanDto>> Handle(ListSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        var items = await _reader.GetAllAsync(null, cancellationToken);
        return items
            .OrderBy(p => p.PlanCode)
            .Select(p => new SubscriptionPlanDto
            {
                Id = p.Id,
                PlanCode = p.PlanCode,
                Name = p.Name,
                Description = p.Description,
                MaxNoteCount = p.MaxNoteCount,
                MonthlyPrice = p.MonthlyPrice,
                YearlyPrice = p.YearlyPrice,
                Currency = p.Currency,
                TrialDays = p.TrialDays,
                IsActive = p.IsActive
            })
            .ToList();
    }
}
