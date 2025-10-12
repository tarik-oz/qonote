using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Abstractions.Subscriptions;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;
using Qonote.Core.Domain.Enums;

namespace Qonote.Core.Application.Features.Subscriptions.ResumeMySubscription;

public sealed class ResumeMySubscriptionCommandHandler : IRequestHandler<ResumeMySubscriptionCommand, Unit>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IReadRepository<UserSubscription, int> _reader;
    private readonly IWriteRepository<UserSubscription, int> _writer;
    private readonly IUnitOfWork _uow;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<ResumeMySubscriptionCommandHandler> _logger;

    public ResumeMySubscriptionCommandHandler(
        ICurrentUserService currentUser,
        IReadRepository<UserSubscription, int> reader,
        IWriteRepository<UserSubscription, int> writer,
        IUnitOfWork uow,
        IPaymentService paymentService,
        ILogger<ResumeMySubscriptionCommandHandler> logger)
    {
        _currentUser = currentUser;
        _reader = reader;
        _writer = writer;
        _uow = uow;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<Unit> Handle(ResumeMySubscriptionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        _logger.LogInformation("ResumeMySubscription started. userId={UserId}", userId);
        var items = await _reader.GetAllAsync(us => us.UserId == userId, cancellationToken);
        var sub = items.OrderByDescending(us => us.StartDate).FirstOrDefault();
        if (sub is null) throw new NotFoundException("No subscription.");

        var now = DateTime.UtcNow;
        var canResume = sub.CancelAtPeriodEnd || (sub.Status == SubscriptionStatus.Cancelled && sub.EndDate != null && sub.EndDate > now);
        if (!canResume)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Subscription", "Subscription is not pending cancellation.")
            });
        }

        if (!string.IsNullOrWhiteSpace(sub.ExternalSubscriptionId))
        {
            _logger.LogDebug("Resuming external subscription. userId={UserId}", userId);
            await _paymentService.ResumeSubscriptionAsync(sub.ExternalSubscriptionId!, cancellationToken);
        }

        sub.CancelAtPeriodEnd = false;
        sub.CancellationReason = null;
        _writer.Update(sub);
        await _uow.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("ResumeMySubscription updated local state. userId={UserId}", userId);

        return Unit.Value;
    }
}


