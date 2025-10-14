using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Admin.PromoCodes.ListPromoCodes;

public sealed class ListPromoCodesQueryHandler : IRequestHandler<ListPromoCodesQuery, IReadOnlyList<PromoCodeListItemDto>>
{
    private readonly IReadRepository<PromoCode, int> _read;
    private readonly IReadRepository<SubscriptionPlan, int> _planRead;
    private readonly ILogger<ListPromoCodesQueryHandler> _logger;

    public ListPromoCodesQueryHandler(
        IReadRepository<PromoCode, int> read,
        IReadRepository<SubscriptionPlan, int> planRead,
        ILogger<ListPromoCodesQueryHandler> logger)
    {
        _read = read;
        _planRead = planRead;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PromoCodeListItemDto>> Handle(ListPromoCodesQuery request, CancellationToken cancellationToken)
    {
        var all = await _read.GetAllAsync(p => true, cancellationToken); // repository abstraction lacks filtering + pagination; do in-memory for now (optimize later)

        var query = all.AsQueryable();
        if (request.IsActive.HasValue)
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim().ToUpperInvariant();
            query = query.Where(p => p.Code.Contains(s));
        }

        // PlanCode filter requires plan lookup mapping PlanId->PlanCode
        Dictionary<int, string> planCodes = new();
        // Load plan codes once (optimizable later if large)
        var plansAll = await _planRead.GetAllAsync(null, cancellationToken);
        planCodes = plansAll.ToDictionary(p => p.Id, p => p.PlanCode);
        if (!string.IsNullOrWhiteSpace(request.PlanCode))
        {
            var targetPlanIds = planCodes.Where(kv => kv.Value == request.PlanCode).Select(kv => kv.Key).ToHashSet();
            query = query.Where(pc => targetPlanIds.Contains(pc.PlanId));
        }

        var skip = (request.Page - 1) * request.PageSize;
        var pageItems = query
            .OrderByDescending(p => p.Id)
            .Skip(skip)
            .Take(request.PageSize)
            .ToList();

        var result = pageItems.Select(pc => new PromoCodeListItemDto(
            pc.Id,
            pc.Code,
            planCodes.TryGetValue(pc.PlanId, out var pcode) ? pcode : string.Empty,
            pc.DurationMonths,
            pc.MaxRedemptions,
            pc.RedemptionCount,
            pc.ExpiresAt,
            pc.IsActive
        )).ToList();

        return result;
    }
}
