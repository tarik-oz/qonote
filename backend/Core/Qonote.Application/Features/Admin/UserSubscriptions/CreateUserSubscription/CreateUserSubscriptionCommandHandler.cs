using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;
using Qonote.Core.Domain.Enums;
using Qonote.Core.Application.Common.Subscriptions;

namespace Qonote.Core.Application.Features.Admin.UserSubscriptions.CreateUserSubscription;

public sealed class CreateUserSubscriptionCommandHandler : IRequestHandler<CreateUserSubscriptionCommand, int>
{
    private readonly IReadRepository<SubscriptionPlan, int> _planReader;
    private readonly IWriteRepository<UserSubscription, int> _writer;
    private readonly IWriteRepository<Payment, int> _paymentWriter;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CreateUserSubscriptionCommandHandler> _logger;

    public CreateUserSubscriptionCommandHandler(
        IReadRepository<SubscriptionPlan, int> planReader,
        IWriteRepository<UserSubscription, int> writer,
        IWriteRepository<Payment, int> paymentWriter,
        IUnitOfWork uow,
        ILogger<CreateUserSubscriptionCommandHandler> logger)
    {
        _planReader = planReader;
        _writer = writer;
        _paymentWriter = paymentWriter;
        _uow = uow;
        _logger = logger;
    }

    public async Task<int> Handle(CreateUserSubscriptionCommand request, CancellationToken cancellationToken)
    {
        // Find plan by code (repository layer filters IsDeleted automatically)
        _logger.LogInformation("Admin CreateUserSubscription started. userId={UserId}, planCode={PlanCode}", request.UserId, request.PlanCode);
        var plans = await _planReader.GetAllAsync(p => p.PlanCode == request.PlanCode, cancellationToken);
        var plan = plans.FirstOrDefault();

        if (plan is null)
        {
            throw new NotFoundException($"Plan '{request.PlanCode}' not found.");
        }

        var entity = new UserSubscription
        {
            UserId = request.UserId,
            PlanId = plan.Id,
            StartDate = request.StartDateUtc,
            EndDate = request.EndDateUtc,
            PriceAmount = request.PriceAmount,
            Currency = request.Currency,
            BillingInterval = request.BillingInterval,
            Status = SubscriptionStatus.Active,
            PaymentProvider = "Manual", // Admin manually created this subscription
            AutoRenew = false // Manual subscriptions don't auto-renew
        };

        // Initialize period window for consistency (used by limits/UI)
        var (ps, pe) = SubscriptionPeriodHelper.ComputeContainingPeriod(
            entity.StartDate,
            entity.BillingInterval,
            DateTime.UtcNow,
            entity.EndDate);
        entity.CurrentPeriodStart = ps;
        entity.CurrentPeriodEnd = pe;

        await _writer.AddAsync(entity, cancellationToken);

        // Parity with webhook/redeem flows: also record a Payment entry
        var payment = new Payment
        {
            UserId = request.UserId,
            UserSubscriptionId = entity.Id,
            Amount = request.PriceAmount,
            Currency = request.Currency,
            Status = PaymentStatus.Succeeded,
            PaymentProvider = "Manual",
            PaidAt = DateTime.UtcNow,
            Description = $"Manual subscription creation for plan {plan.PlanCode}"
        };
        await _paymentWriter.AddAsync(payment, cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Admin CreateUserSubscription created. userId={UserId}, subscriptionId={SubscriptionId}", request.UserId, entity.Id);
        return entity.Id;
    }
}
