using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.Delete;

public sealed class DeleteSubscriptionPlanCommandHandler : IRequestHandler<DeleteSubscriptionPlanCommand>
{
    private readonly IReadRepository<SubscriptionPlan, int> _reader;
    private readonly IWriteRepository<SubscriptionPlan, int> _writer;
    private readonly IUnitOfWork _uow;

    public DeleteSubscriptionPlanCommandHandler(
        IReadRepository<SubscriptionPlan, int> reader,
        IWriteRepository<SubscriptionPlan, int> writer,
        IUnitOfWork uow)
    {
        _reader = reader;
        _writer = writer;
        _uow = uow;
    }

    public async Task Handle(DeleteSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        var entity = await _reader.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            throw new NotFoundException($"SubscriptionPlan {request.Id} not found.");

        _writer.Delete(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
