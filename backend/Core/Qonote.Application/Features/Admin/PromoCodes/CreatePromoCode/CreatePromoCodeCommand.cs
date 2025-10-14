using MediatR;

namespace Qonote.Core.Application.Features.Admin.PromoCodes.CreatePromoCode;

public sealed record CreatePromoCodeCommand(
    string Code,
    string PlanCode,
    int DurationMonths,
    int? MaxRedemptions,
    DateTime? ExpiresAtUtc,
    bool SingleUsePerUser,
    string? Description
) : IRequest<int>;
