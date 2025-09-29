using MediatR;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.Delete;

public sealed record DeleteSubscriptionPlanCommand(
    int Id
) : IRequest;
