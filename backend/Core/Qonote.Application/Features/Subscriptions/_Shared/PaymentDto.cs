using Qonote.Core.Domain.Enums;

namespace Qonote.Core.Application.Features.Subscriptions._Shared;

public record PaymentDto
{
    public int Id { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public PaymentStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? PaidAt { get; init; }
    public string? Description { get; init; }
    public string? InvoiceUrl { get; init; }
}

