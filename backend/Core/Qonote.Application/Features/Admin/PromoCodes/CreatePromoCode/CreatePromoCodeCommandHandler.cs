using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Admin.PromoCodes.CreatePromoCode;

public sealed class CreatePromoCodeCommandHandler : IRequestHandler<CreatePromoCodeCommand, int>
{
    private readonly IReadRepository<SubscriptionPlan, int> _planReader;
    private readonly IReadRepository<PromoCode, int> _promoRead;
    private readonly IWriteRepository<PromoCode, int> _promoWrite;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CreatePromoCodeCommandHandler> _logger;

    public CreatePromoCodeCommandHandler(
        IReadRepository<SubscriptionPlan, int> planReader,
        IReadRepository<PromoCode, int> promoRead,
        IWriteRepository<PromoCode, int> promoWrite,
        IUnitOfWork uow,
        ILogger<CreatePromoCodeCommandHandler> logger)
    {
        _planReader = planReader;
        _promoRead = promoRead;
        _promoWrite = promoWrite;
        _uow = uow;
        _logger = logger;
    }

    public async Task<int> Handle(CreatePromoCodeCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        _logger.LogInformation("Creating promo code {Code} planCode={PlanCode}", code, request.PlanCode);

        // Plan lookup by PlanCode
        var plans = await _planReader.GetAllAsync(p => p.PlanCode == request.PlanCode, cancellationToken);
        var plan = plans.FirstOrDefault();
        if (plan is null)
        {
            throw new NotFoundException($"Plan '{request.PlanCode}' not found.");
        }

        // Uniqueness check (defensive; DB unique index also exists)
        var existing = await _promoRead.GetAllAsync(p => p.Code == code, cancellationToken);
        if (existing.Any())
        {
            throw new ConflictException($"Promo code '{code}' already exists.");
        }

        var entity = new PromoCode
        {
            Code = code,
            PlanId = plan.Id,
            DurationMonths = request.DurationMonths,
            MaxRedemptions = request.MaxRedemptions,
            ExpiresAt = request.ExpiresAtUtc,
            SingleUsePerUser = request.SingleUsePerUser,
            Description = request.Description,
            IsActive = true
        };

        await _promoWrite.AddAsync(entity, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Promo code created id={Id} code={Code}", entity.Id, entity.Code);
        return entity.Id;
    }
}
