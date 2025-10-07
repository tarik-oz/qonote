using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Abstractions.Subscriptions;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Subscriptions.CancelMySubscription;

public sealed class CancelMySubscriptionCommandHandler : IRequestHandler<CancelMySubscriptionCommand, Unit>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IReadRepository<UserSubscription, int> _reader;
    private readonly IWriteRepository<UserSubscription, int> _writer;
    private readonly IUnitOfWork _uow;
    private readonly IPaymentService _paymentService;

    public CancelMySubscriptionCommandHandler(
        ICurrentUserService currentUser,
        IReadRepository<UserSubscription, int> reader,
        IWriteRepository<UserSubscription, int> writer,
        IUnitOfWork uow,
        IPaymentService paymentService)
    {
        _currentUser = currentUser;
        _reader = reader;
        _writer = writer;
        _uow = uow;
        _paymentService = paymentService;
    }

    public async Task<Unit> Handle(CancelMySubscriptionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var items = await _reader.GetAllAsync(us => us.UserId == userId, cancellationToken);
        var sub = items.OrderByDescending(us => us.StartDate).FirstOrDefault();
        if (sub is null) throw new NotFoundException("No active subscription.");

        if (!string.IsNullOrWhiteSpace(sub.ExternalSubscriptionId))
        {
            await _paymentService.CancelSubscriptionAsync(sub.ExternalSubscriptionId!, cancellationToken);
        }

        sub.CancelAtPeriodEnd = request.CancelAtPeriodEnd;
        sub.CancellationReason = request.Reason;
        _writer.Update(sub);
        await _uow.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}


