using MediatR;

namespace Qonote.Core.Application.Features.Admin.UserSubscriptions.ListUserSubscriptions;

public sealed record ListUserSubscriptionsQuery(
    string UserId
) : IRequest<List<UserSubscriptionDto>>;
