using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.CreateSubscriptionPlan;

public sealed class CreateSubscriptionPlanCommandHandler : IRequestHandler<CreateSubscriptionPlanCommand, int>
{
    private readonly IReadRepository<SubscriptionPlan, int> _reader;
    private readonly IWriteRepository<SubscriptionPlan, int> _writer;
    private readonly IUnitOfWork _uow;

    public CreateSubscriptionPlanCommandHandler(
        IReadRepository<SubscriptionPlan, int> reader,
        IWriteRepository<SubscriptionPlan, int> writer,
        IUnitOfWork uow)
    {
        _reader = reader;
        _writer = writer;
        _uow = uow;
    }

    public async Task<int> Handle(CreateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        // Ensure unique PlanCode among non-deleted plans
        var exists = (await _reader.GetAllAsync(p => p.PlanCode == request.PlanCode, cancellationToken)).Any();
        if (exists)
        {
            var failures = new[]
            {
                new FluentValidation.Results.ValidationFailure("PlanCode", "PlanCode must be unique.")
            };
            throw new ValidationException(failures);
        }

        var entity = new SubscriptionPlan
        {
            PlanCode = request.PlanCode,
            Name = request.Name,
            MaxNoteCount = request.MaxNoteCount
        };
        await _writer.AddAsync(entity, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
