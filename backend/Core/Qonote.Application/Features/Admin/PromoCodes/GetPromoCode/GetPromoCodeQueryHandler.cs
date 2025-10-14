using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Admin.PromoCodes.GetPromoCode;

public sealed class GetPromoCodeQueryHandler : IRequestHandler<GetPromoCodeQuery, PromoCodeDto>
{
    private readonly IReadRepository<PromoCode, int> _read;
    private readonly IReadRepository<SubscriptionPlan, int> _planRead;
    private readonly ILogger<GetPromoCodeQueryHandler> _logger;

    public GetPromoCodeQueryHandler(
        IReadRepository<PromoCode, int> read,
        IReadRepository<SubscriptionPlan, int> planRead,
        ILogger<GetPromoCodeQueryHandler> logger)
    {
        _read = read;
        _planRead = planRead;
        _logger = logger;
    }

    public async Task<PromoCodeDto> Handle(GetPromoCodeQuery request, CancellationToken cancellationToken)
    {
        var list = await _read.GetAllAsync(p => p.Id == request.Id, cancellationToken);
        var entity = list.FirstOrDefault();
        if (entity is null)
            throw new NotFoundException($"Promo code {request.Id} not found");

        var plans = await _planRead.GetAllAsync(p => p.Id == entity.PlanId, cancellationToken);
        var plan = plans.FirstOrDefault();
        var planCode = plan?.PlanCode ?? string.Empty;

        return new PromoCodeDto(
            entity.Id,
            entity.Code,
            planCode,
            entity.DurationMonths,
            entity.MaxRedemptions,
            entity.RedemptionCount,
            entity.ExpiresAt,
            entity.SingleUsePerUser,
            entity.IsActive,
            entity.Description,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}
