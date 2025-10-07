using MediatR;
using Qonote.Core.Application.Features.Subscriptions._Shared;

namespace Qonote.Core.Application.Features.Subscriptions.GetMySubscription;

public sealed record GetMySubscriptionQuery() : IRequest<UserSubscriptionDto?>;


