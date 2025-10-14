using MediatR;

namespace Qonote.Core.Application.Features.Admin.PromoCodes.GetPromoCode;

public sealed record GetPromoCodeQuery(int Id) : IRequest<PromoCodeDto>;

public sealed record PromoCodeDto(
    int Id,
    string Code,
    string PlanCode,
    int DurationMonths,
    int? MaxRedemptions,
    int RedemptionCount,
    DateTime? ExpiresAtUtc,
    bool SingleUsePerUser,
    bool IsActive,
    string? Description,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);
