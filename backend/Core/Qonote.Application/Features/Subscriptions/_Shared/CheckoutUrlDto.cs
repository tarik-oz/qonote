namespace Qonote.Core.Application.Features.Subscriptions._Shared;

public sealed record CheckoutUrlDto
{
    public string CheckoutUrl { get; init; } = string.Empty;
}

