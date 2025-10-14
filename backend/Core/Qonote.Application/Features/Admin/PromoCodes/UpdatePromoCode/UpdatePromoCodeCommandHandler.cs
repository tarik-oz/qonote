using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Admin.PromoCodes.UpdatePromoCode;

public sealed class UpdatePromoCodeCommandHandler : IRequestHandler<UpdatePromoCodeCommand>
{
    private readonly IReadRepository<PromoCode, int> _read;
    private readonly IReadRepository<SubscriptionPlan, int> _planRead;
    private readonly IWriteRepository<PromoCode, int> _write;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<UpdatePromoCodeCommandHandler> _logger;

    public UpdatePromoCodeCommandHandler(
        IReadRepository<PromoCode, int> read,
        IReadRepository<SubscriptionPlan, int> planRead,
        IWriteRepository<PromoCode, int> write,
        IUnitOfWork uow,
        ILogger<UpdatePromoCodeCommandHandler> logger)
    {
        _read = read;
        _planRead = planRead;
        _write = write;
        _uow = uow;
        _logger = logger;
    }

    public async Task Handle(UpdatePromoCodeCommand request, CancellationToken cancellationToken)
    {
        var list = await _read.GetAllAsync(p => p.Id == request.Id, cancellationToken);
        var entity = list.FirstOrDefault();
        if (entity is null)
            throw new NotFoundException($"Promo code {request.Id} not found");

        if (request.PlanCode is not null)
        {
            var plans = await _planRead.GetAllAsync(p => p.PlanCode == request.PlanCode, cancellationToken);
            var plan = plans.FirstOrDefault();
            if (plan is null)
                throw new NotFoundException($"Plan '{request.PlanCode}' not found.");
            entity.PlanId = plan.Id;
        }

        if (request.DurationMonths.HasValue)
            entity.DurationMonths = request.DurationMonths.Value;

        if (request.MaxRedemptions.HasValue)
        {
            if (request.MaxRedemptions.Value < entity.RedemptionCount)
                throw new ConflictException("MaxRedemptions cannot be less than current RedemptionCount");
            entity.MaxRedemptions = request.MaxRedemptions.Value;
        }

        if (request.ExpiresAtUtc.HasValue)
            entity.ExpiresAt = request.ExpiresAtUtc;

        if (request.SingleUsePerUser.HasValue)
        {
            entity.SingleUsePerUser = request.SingleUsePerUser.Value;
        }

        if (request.IsActive.HasValue)
        {
            if (entity.IsActive && request.IsActive == false)
                entity.DeactivatedAt = DateTime.UtcNow;
            if (!entity.IsActive && request.IsActive == true)
                entity.DeactivatedAt = null;
            entity.IsActive = request.IsActive.Value;
        }

        if (request.Description is not null)
        {
            entity.Description = request.Description;
        }

        entity.UpdatedAt = DateTime.UtcNow;
        _write.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Promo code updated id={Id}", entity.Id);
    }
}
