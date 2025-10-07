using MediatR;
using Qonote.Core.Application.Features.Subscriptions._Shared;

namespace Qonote.Core.Application.Features.Subscriptions.ListPublicPlans;

public sealed record ListPublicPlansQuery() : IRequest<List<SubscriptionPlanDto>>;


