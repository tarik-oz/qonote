using MediatR;

namespace Qonote.Core.Application.Features.Admin.PromoCodes.UpdatePromoCode;

public sealed record UpdatePromoCodeCommand(
    int Id,
    string? PlanCode,
    int? DurationMonths,
    int? MaxRedemptions,
    DateTime? ExpiresAtUtc,
    bool? SingleUsePerUser,
    bool? IsActive,
    string? Description
) : IRequest;
