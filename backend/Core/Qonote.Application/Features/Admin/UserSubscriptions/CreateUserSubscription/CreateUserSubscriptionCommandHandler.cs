using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Admin.UserSubscriptions.CreateUserSubscription;

public sealed class CreateUserSubscriptionCommandHandler : IRequestHandler<CreateUserSubscriptionCommand, int>
{
    private readonly IReadRepository<SubscriptionPlan, int> _planReader;
    private readonly IWriteRepository<UserSubscription, int> _writer;
    private readonly IUnitOfWork _uow;

    public CreateUserSubscriptionCommandHandler(
        IReadRepository<SubscriptionPlan, int> planReader,
        IWriteRepository<UserSubscription, int> writer,
        IUnitOfWork uow)
    {
        _planReader = planReader;
        _writer = writer;
        _uow = uow;
    }

    public async Task<int> Handle(CreateUserSubscriptionCommand request, CancellationToken cancellationToken)
    {
        // Find plan by code (repository layer filters IsDeleted automatically)
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
            BillingPeriod = request.BillingPeriod
        };

        await _writer.AddAsync(entity, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
