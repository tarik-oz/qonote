using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Features.Subscriptions._Shared;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.ListSubscriptionPlans;

public sealed class ListSubscriptionPlansQueryHandler : IRequestHandler<ListSubscriptionPlansQuery, List<SubscriptionPlanDto>>
{
    private readonly IReadRepository<SubscriptionPlan, int> _reader;
    private readonly ILogger<ListSubscriptionPlansQueryHandler> _logger;
    public ListSubscriptionPlansQueryHandler(IReadRepository<SubscriptionPlan, int> reader, ILogger<ListSubscriptionPlansQueryHandler> logger)
    {
        _reader = reader;
        _logger = logger;
    }

    public async Task<List<SubscriptionPlanDto>> Handle(ListSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Admin ListSubscriptionPlans started");
        var items = await _reader.GetAllAsync(null, cancellationToken);
        var result = items
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
        _logger.LogDebug("Admin ListSubscriptionPlans returning {Count} plans", result.Count);
        return result;
    }
}
