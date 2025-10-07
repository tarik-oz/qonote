using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Features.Subscriptions._Shared;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Subscriptions.ListMyPayments;

public sealed class ListMyPaymentsQueryHandler : IRequestHandler<ListMyPaymentsQuery, List<PaymentDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IReadRepository<Payment, int> _reader;

    public ListMyPaymentsQueryHandler(ICurrentUserService currentUser, IReadRepository<Payment, int> reader)
    {
        _currentUser = currentUser;
        _reader = reader;
    }

    public async Task<List<PaymentDto>> Handle(ListMyPaymentsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var items = await _reader.GetAllAsync(p => p.UserId == userId, cancellationToken);
        return items
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                Amount = p.Amount,
                Currency = p.Currency,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                PaidAt = p.PaidAt,
                Description = p.Description,
                InvoiceUrl = p.InvoiceUrl
            })
            .ToList();
    }
}


