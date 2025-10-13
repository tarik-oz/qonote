using MediatR;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.UpdateSubscriptionPlan;

public sealed record UpdateSubscriptionPlanCommand(
    int Id,
    string PlanCode,
    string Name,
    int MaxNoteCount,
    string? Description,
    decimal MonthlyPrice,
    decimal YearlyPrice,
    string Currency,
    int TrialDays,
    bool IsActive,
    string? ExternalProductId,
    string? ExternalPriceIdMonthly,
    string? ExternalPriceIdYearly
) : IRequest;
