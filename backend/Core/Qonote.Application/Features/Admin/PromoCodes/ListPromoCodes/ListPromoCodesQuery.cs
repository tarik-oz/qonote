using MediatR;

namespace Qonote.Core.Application.Features.Admin.PromoCodes.ListPromoCodes;

public sealed record ListPromoCodesQuery(
    bool? IsActive,
    string? PlanCode,
    string? Search,
    int Page = 1,
    int PageSize = 50
) : IRequest<IReadOnlyList<PromoCodeListItemDto>>;

public sealed record PromoCodeListItemDto(
    int Id,
    string Code,
    string PlanCode,
    int DurationMonths,
    int? MaxRedemptions,
    int RedemptionCount,
    DateTime? ExpiresAtUtc,
    bool IsActive
);
