using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Admin.UserSubscriptions.CreateUserSubscription;

public sealed class CreateUserSubscriptionCommandHandler : IRequestHandler<CreateUserSubscriptionCommand, int>
{
    private readonly IReadRepository<SubscriptionPlan, int> _planReader;
    private readonly IWriteRepository<UserSubscription, int> _writer;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CreateUserSubscriptionCommandHandler> _logger;

    public CreateUserSubscriptionCommandHandler(
        IReadRepository<SubscriptionPlan, int> planReader,
        IWriteRepository<UserSubscription, int> writer,
        IUnitOfWork uow,
        ILogger<CreateUserSubscriptionCommandHandler> logger)
    {
        _planReader = planReader;
        _writer = writer;
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
            Status = Domain.Enums.SubscriptionStatus.Active,
            PaymentProvider = "Manual", // Admin manually created this subscription
            AutoRenew = false // Manual subscriptions don't auto-renew
        };

        await _writer.AddAsync(entity, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Admin CreateUserSubscription created. userId={UserId}, subscriptionId={SubscriptionId}", request.UserId, entity.Id);
        return entity.Id;
    }
}
