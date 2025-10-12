using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Features.Subscriptions._Shared;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Subscriptions.ListPublicPlans;

public sealed class ListPublicPlansQueryHandler : IRequestHandler<ListPublicPlansQuery, List<SubscriptionPlanDto>>
{
    private readonly IReadRepository<SubscriptionPlan, int> _reader;
    private readonly ILogger<ListPublicPlansQueryHandler> _logger;
    public ListPublicPlansQueryHandler(IReadRepository<SubscriptionPlan, int> reader, ILogger<ListPublicPlansQueryHandler> logger)
    {
        _reader = reader;
        _logger = logger;
    }

    public async Task<List<SubscriptionPlanDto>> Handle(ListPublicPlansQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("ListPublicPlans started");
        var items = await _reader.GetAllAsync(p => p.IsActive, cancellationToken);
        return items
            .OrderBy(p => p.MonthlyPrice)
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


