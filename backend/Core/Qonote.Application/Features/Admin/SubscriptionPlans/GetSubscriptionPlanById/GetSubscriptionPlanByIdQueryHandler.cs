using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Features.Subscriptions._Shared;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.GetSubscriptionPlanById;

public sealed class GetSubscriptionPlanByIdQueryHandler : IRequestHandler<GetSubscriptionPlanByIdQuery, SubscriptionPlanDto?>
{
    private readonly IReadRepository<SubscriptionPlan, int> _reader;
    private readonly ILogger<GetSubscriptionPlanByIdQueryHandler> _logger;
    public GetSubscriptionPlanByIdQueryHandler(IReadRepository<SubscriptionPlan, int> reader, ILogger<GetSubscriptionPlanByIdQueryHandler> logger)
    {
        _reader = reader;
        _logger = logger;
    }

    public async Task<SubscriptionPlanDto?> Handle(GetSubscriptionPlanByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Admin GetSubscriptionPlanById started. planId={PlanId}", request.Id);
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
