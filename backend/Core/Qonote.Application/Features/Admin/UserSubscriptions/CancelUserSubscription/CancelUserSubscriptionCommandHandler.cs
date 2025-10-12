using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Caching;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;
using Qonote.Core.Domain.Enums;

namespace Qonote.Core.Application.Features.Admin.UserSubscriptions.CancelUserSubscription;

public sealed class CancelUserSubscriptionCommandHandler : IRequestHandler<CancelUserSubscriptionCommand, Unit>
{
    private readonly IReadRepository<UserSubscription, int> _reader;
    private readonly IWriteRepository<UserSubscription, int> _writer;
    private readonly IUnitOfWork _uow;
    private readonly ICacheInvalidationService _cacheInvalidation;
    private readonly ILogger<CancelUserSubscriptionCommandHandler> _logger;

    public CancelUserSubscriptionCommandHandler(
        IReadRepository<UserSubscription, int> reader,
        IWriteRepository<UserSubscription, int> writer,
        IUnitOfWork uow,
        ICacheInvalidationService cacheInvalidation,
        ILogger<CancelUserSubscriptionCommandHandler> logger)
    {
        _reader = reader;
        _writer = writer;
        _uow = uow;
        _cacheInvalidation = cacheInvalidation;
        _logger = logger;
    }

    public async Task<Unit> Handle(CancelUserSubscriptionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin CancelUserSubscription started. userId={UserId}, subscriptionId={SubscriptionId}, atPeriodEnd={AtPeriodEnd}", request.UserId, request.SubscriptionId, request.CancelAtPeriodEnd);
        var sub = await _reader.GetByIdAsync(request.SubscriptionId, cancellationToken);
        if (sub is null || sub.UserId != request.UserId)
        {
            throw new NotFoundException("Subscription not found.");
        }

        if (request.CancelAtPeriodEnd && sub.CurrentPeriodEnd.HasValue)
        {
            sub.CancelAtPeriodEnd = true;
            sub.CancellationReason = request.Reason;
        }
        else
        {
            sub.Status = SubscriptionStatus.Cancelled;
            sub.CancelledAt = DateTime.UtcNow;
            sub.EndDate = DateTime.UtcNow;
            sub.CancelAtPeriodEnd = false;
            sub.CancellationReason = request.Reason;
            sub.AutoRenew = false;
        }

        _writer.Update(sub);
        await _uow.SaveChangesAsync(cancellationToken);

        await _cacheInvalidation.RemoveMeAsync(request.UserId, cancellationToken);
        _logger.LogInformation("Admin CancelUserSubscription updated. userId={UserId}, subscriptionId={SubscriptionId}", request.UserId, request.SubscriptionId);
        return Unit.Value;
    }
}


