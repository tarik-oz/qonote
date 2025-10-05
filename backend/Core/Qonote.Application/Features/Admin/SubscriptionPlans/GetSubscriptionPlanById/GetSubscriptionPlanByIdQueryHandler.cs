using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Features.Subscriptions._Shared;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.GetSubscriptionPlanById;

public sealed class GetSubscriptionPlanByIdQueryHandler : IRequestHandler<GetSubscriptionPlanByIdQuery, SubscriptionPlanDto?>
{
    private readonly IReadRepository<SubscriptionPlan, int> _reader;
    public GetSubscriptionPlanByIdQueryHandler(IReadRepository<SubscriptionPlan, int> reader) => _reader = reader;

    public async Task<SubscriptionPlanDto?> Handle(GetSubscriptionPlanByIdQuery request, CancellationToken cancellationToken)
    {
        var plan = await _reader.GetByIdAsync(request.Id, cancellationToken);
        if (plan is null) return null;
        return new SubscriptionPlanDto
        {
            Id = plan.Id,
            PlanCode = plan.PlanCode,
            Name = plan.Name,
            Description = plan.Description,
            MaxNoteCount = plan.MaxNoteCount,
            MonthlyPrice = plan.MonthlyPrice,
            YearlyPrice = plan.YearlyPrice,
            Currency = plan.Currency,
            TrialDays = plan.TrialDays,
            IsActive = plan.IsActive
        };
    }
}
