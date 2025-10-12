using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Abstractions.Subscriptions;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;
using Qonote.Core.Domain.Enums;

namespace Qonote.Core.Application.Features.Subscriptions.CancelMySubscription;

public sealed class CancelMySubscriptionCommandHandler : IRequestHandler<CancelMySubscriptionCommand, Unit>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IReadRepository<UserSubscription, int> _reader;
    private readonly IWriteRepository<UserSubscription, int> _writer;
    private readonly IUnitOfWork _uow;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<CancelMySubscriptionCommandHandler> _logger;

    public CancelMySubscriptionCommandHandler(
        ICurrentUserService currentUser,
        IReadRepository<UserSubscription, int> reader,
        IWriteRepository<UserSubscription, int> writer,
        IUnitOfWork uow,
        IPaymentService paymentService,
        ILogger<CancelMySubscriptionCommandHandler> logger)
    {
        _currentUser = currentUser;
        _reader = reader;
        _writer = writer;
        _uow = uow;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<Unit> Handle(CancelMySubscriptionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        _logger.LogInformation("CancelMySubscription started. userId={UserId}, atPeriodEnd={AtPeriodEnd}", userId, request.CancelAtPeriodEnd);
        var items = await _reader.GetAllAsync(us => us.UserId == userId, cancellationToken);
        var sub = items.OrderByDescending(us => us.StartDate).FirstOrDefault();
        if (sub is null) throw new NotFoundException("No active subscription.");

        // Guard: only allow cancel if currently active/trialing or scheduled to renew
        var now = DateTime.UtcNow;
        var isEffectivelyActive = (sub.Status == SubscriptionStatus.Active || sub.Status == SubscriptionStatus.Trialing)
                                  && (sub.EndDate == null || sub.EndDate > now);
        if (!isEffectivelyActive && !sub.CancelAtPeriodEnd)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Subscription", "Subscription is not active or already canceled.")
            });
        }

        if (!string.IsNullOrWhiteSpace(sub.ExternalSubscriptionId))
        {
            _logger.LogDebug("Cancelling external subscription. userId={UserId}", userId);
            await _paymentService.CancelSubscriptionAsync(sub.ExternalSubscriptionId!, cancellationToken);
        }

        sub.CancelAtPeriodEnd = request.CancelAtPeriodEnd;
        sub.CancellationReason = request.Reason;
        _writer.Update(sub);
        await _uow.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("CancelMySubscription updated local state. userId={UserId}, atPeriodEnd={AtPeriodEnd}", userId, request.CancelAtPeriodEnd);

        return Unit.Value;
    }
}


