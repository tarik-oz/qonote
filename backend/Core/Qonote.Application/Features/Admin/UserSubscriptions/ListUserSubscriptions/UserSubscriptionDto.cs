namespace Qonote.Core.Application.Features.Admin.UserSubscriptions.ListUserSubscriptions;

public sealed record UserSubscriptionDto(
    int Id,
    string PlanCode,
    DateTime StartDateUtc,
    DateTime EndDateUtc,
    decimal? PriceAmount,
    string? Currency,
    string? BillingPeriod
);
