using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.DeleteSubscriptionPlan;

public sealed class DeleteSubscriptionPlanCommandHandler : IRequestHandler<DeleteSubscriptionPlanCommand>
{
    private readonly IReadRepository<SubscriptionPlan, int> _reader;
    private readonly IWriteRepository<SubscriptionPlan, int> _writer;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<DeleteSubscriptionPlanCommandHandler> _logger;

    public DeleteSubscriptionPlanCommandHandler(
        IReadRepository<SubscriptionPlan, int> reader,
        IWriteRepository<SubscriptionPlan, int> writer,
        IUnitOfWork uow,
        ILogger<DeleteSubscriptionPlanCommandHandler> logger)
    {
        _reader = reader;
        _writer = writer;
        _uow = uow;
        _logger = logger;
    }

    public async Task Handle(DeleteSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin DeleteSubscriptionPlan started. planId={PlanId}", request.Id);
        var entity = await _reader.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            throw new NotFoundException($"SubscriptionPlan {request.Id} not found.");

        _writer.Delete(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Admin DeleteSubscriptionPlan deleted. planId={PlanId}", request.Id);
    }
}
