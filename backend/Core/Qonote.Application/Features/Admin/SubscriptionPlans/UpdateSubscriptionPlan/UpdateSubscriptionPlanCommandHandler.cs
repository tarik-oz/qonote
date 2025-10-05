using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.UpdateSubscriptionPlan;

public sealed class UpdateSubscriptionPlanCommandHandler : IRequestHandler<UpdateSubscriptionPlanCommand>
{
    private readonly IReadRepository<SubscriptionPlan, int> _reader;
    private readonly IWriteRepository<SubscriptionPlan, int> _writer;
    private readonly IUnitOfWork _uow;

    public UpdateSubscriptionPlanCommandHandler(
        IReadRepository<SubscriptionPlan, int> reader,
        IWriteRepository<SubscriptionPlan, int> writer,
        IUnitOfWork uow)
    {
        _reader = reader;
        _writer = writer;
        _uow = uow;
    }

    public async Task Handle(UpdateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        var entity = await _reader.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            throw new NotFoundException($"SubscriptionPlan {request.Id} not found.");

        // Uniqueness check for PlanCode (excluding self)
        var others = await _reader.GetAllAsync(p => p.PlanCode == request.PlanCode && p.Id != request.Id, cancellationToken);
        if (others.Any())
        {
            var failures = new[]
            {
                new FluentValidation.Results.ValidationFailure("PlanCode", "PlanCode must be unique.")
            };
            throw new ValidationException(failures);
        }

        entity.PlanCode = request.PlanCode;
        entity.Name = request.Name;
        entity.MaxNoteCount = request.MaxNoteCount;

        _writer.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
