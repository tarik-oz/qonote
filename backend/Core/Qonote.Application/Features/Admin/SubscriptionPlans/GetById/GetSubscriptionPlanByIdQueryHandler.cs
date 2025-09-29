using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Features.Admin.SubscriptionPlans._Shared;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.GetById;

public sealed class GetSubscriptionPlanByIdQueryHandler : IRequestHandler<GetSubscriptionPlanByIdQuery, SubscriptionPlanDto?>
{
    private readonly IReadRepository<SubscriptionPlan, int> _reader;
    public GetSubscriptionPlanByIdQueryHandler(IReadRepository<SubscriptionPlan, int> reader) => _reader = reader;

    public async Task<SubscriptionPlanDto?> Handle(GetSubscriptionPlanByIdQuery request, CancellationToken cancellationToken)
    {
        var plan = await _reader.GetByIdAsync(request.Id, cancellationToken);
        if (plan is null) return null;
        return new SubscriptionPlanDto(plan.Id, plan.PlanCode, plan.Name, plan.MaxNoteCount);
    }
}
