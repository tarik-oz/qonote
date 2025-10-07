using MediatR;
using Qonote.Core.Application.Features.Subscriptions._Shared;
using Qonote.Core.Domain.Enums;

namespace Qonote.Core.Application.Features.Subscriptions.CreateCheckout;

public sealed record CreateCheckoutCommand(
    int PlanId,
    BillingInterval BillingInterval,
    string? SuccessUrl,
    string? CancelUrl
) : IRequest<CheckoutUrlDto>;


