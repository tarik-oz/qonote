using MediatR;
using Qonote.Core.Application.Features.Admin.SubscriptionPlans._Shared;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.List;

public sealed record ListSubscriptionPlansQuery() : IRequest<List<SubscriptionPlanDto>>;
