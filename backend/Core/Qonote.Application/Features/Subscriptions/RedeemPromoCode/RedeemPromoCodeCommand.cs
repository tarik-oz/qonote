using MediatR;

namespace Qonote.Core.Application.Features.Subscriptions.RedeemPromoCode;

public sealed record RedeemPromoCodeCommand(
    string Code
) : IRequest<int>; // returns created UserSubscriptionId
