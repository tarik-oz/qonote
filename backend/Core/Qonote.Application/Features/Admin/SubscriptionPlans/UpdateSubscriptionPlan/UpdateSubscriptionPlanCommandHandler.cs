using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.UpdateSubscriptionPlan;

public sealed class UpdateSubscriptionPlanCommandHandler : IRequestHandler<UpdateSubscriptionPlanCommand>
{
    private readonly IReadRepository<SubscriptionPlan, int> _reader;
    private readonly IWriteRepository<SubscriptionPlan, int> _writer;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<UpdateSubscriptionPlanCommandHandler> _logger;

    public UpdateSubscriptionPlanCommandHandler(
        IReadRepository<SubscriptionPlan, int> reader,
        IWriteRepository<SubscriptionPlan, int> writer,
        IUnitOfWork uow,
        ILogger<UpdateSubscriptionPlanCommandHandler> logger)
    {
        _reader = reader;
        _writer = writer;
        _uow = uow;
        _logger = logger;
    }

    public async Task Handle(UpdateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin UpdateSubscriptionPlan started. planId={PlanId}", request.Id);
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
        entity.Description = request.Description;
        entity.MonthlyPrice = request.MonthlyPrice;
        entity.YearlyPrice = request.YearlyPrice;
        entity.Currency = request.Currency;
        entity.TrialDays = request.TrialDays;
        entity.IsActive = request.IsActive;
        entity.ExternalProductId = request.ExternalProductId;
        entity.ExternalPriceIdMonthly = request.ExternalPriceIdMonthly;
        entity.ExternalPriceIdYearly = request.ExternalPriceIdYearly;

        _writer.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Admin UpdateSubscriptionPlan updated. planId={PlanId}", request.Id);
    }
}
