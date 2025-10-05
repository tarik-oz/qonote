using MediatR;
using Qonote.Core.Application.Features.Subscriptions._Shared;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.ListSubscriptionPlans;

public sealed record ListSubscriptionPlansQuery() : IRequest<List<SubscriptionPlanDto>>;
