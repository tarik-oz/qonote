using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Features.Admin.SubscriptionPlans._Shared;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.List;

public sealed class ListSubscriptionPlansQueryHandler : IRequestHandler<ListSubscriptionPlansQuery, List<SubscriptionPlanDto>>
{
    private readonly IReadRepository<SubscriptionPlan, int> _reader;
    public ListSubscriptionPlansQueryHandler(IReadRepository<SubscriptionPlan, int> reader) => _reader = reader;

    public async Task<List<SubscriptionPlanDto>> Handle(ListSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        var items = await _reader.GetAllAsync(null, cancellationToken);
        return items
            .OrderBy(p => p.PlanCode)
            .Select(p => new SubscriptionPlanDto(p.Id, p.PlanCode, p.Name, p.MaxNoteCount))
            .ToList();
    }
}
